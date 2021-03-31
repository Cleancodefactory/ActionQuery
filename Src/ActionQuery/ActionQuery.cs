﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ccf.Ck.SysPlugins.Support.ActionQuery
{
    public class ActionQuery<ResolverValue> where ResolverValue: new() {

        enum Terms {
            
            none = 0,
            // Space is found - usually ignored
            space = 1,
            // special literals for specific values - true, false, null, value
            specialliteral = 2,
            // Operator from the langlet
            keyword = 3,
            // identifier - function name or parameter name to fetch (the actual fetching depends on the usage)
            identifier = 4,
            // Open normal bracket (function call arguments, grouping is not supported intentionally - see the docs for more details)
            openbracket = 5,
            // close normal bracket - end of function call argument list.
            closebracket = 6,
            // string literal 'something'
            stringliteral = 7,
            // numeric literal like: 124, +234, -324, 123.45, -2.43, +0.23423 etc.
            numliteral = 8,
            // comma separator of arguments. can be used at top level also, in this case this will produce multiple results (usable only with the corresponding evaluation routines)
            comma = 9,
            // end of the expression
            end = 10,
            // Virtual tokens ===
            compound = 101
        }
        private static readonly Regex _regex = new Regex(@"(\s+)|(true|false|null)|(while|if)|([a-zA-Z_\-][a-zA-Z0-9_\.]*)|(\()|(\))|(?:\'((?:\\'|[^\'])*)\')|([\+\-]?\d+(?:\.\d*)?)|(\,|(?:\r|\n)+)|($)",
            RegexOptions.None);


        private struct OpEntry {
            internal OpEntry(string v, Terms t,int pos = -1) {
                Value = v;
                Term = t;
                Pos = pos;
                Arguments = 0;
                Addresses = new List<int>();
            }
            internal string Value;
            internal Terms Term;
            internal int Pos;

            internal int Arguments;

            internal List<int> Addresses;

            public bool IsEmpty { get {
                return (Term == Terms.none);
            }}
            public static OpEntry Empty {
                get {
                    return new OpEntry(null, Terms.none);
                }
            }

        }

        #region Internal helpers (can be implemented as local methods, but I hate it)
        
        private int ParsePos(Match m) {
            return m.Index;
        }
        private string ReportError(string fmt,Match m = null) {
            if (m != null) {
                return string.Format(fmt,ParsePos(m));
            } 
            return fmt;
        }
        private string ReportError(string fmt,int m = -1) {
            if (m >= 0) {
                return string.Format(fmt,m);
            } 
            return fmt;
        }
        private void AddArg(Stack<OpEntry> stack, ActionQueryRunner<ResolverValue>.Constructor constr) {
            if (stack.Count > 0) {
                var entry = stack.Peek();
                entry.Arguments ++;
                entry.Addresses.Add(constr.Address);
            }
        }
        #endregion
        public ActionQueryRunner<ResolverValue> Compile(string query) {

            Stack<OpEntry> opstack = new Stack<OpEntry>();
            ActionQueryRunner<ResolverValue>.Constructor runner = new ActionQueryRunner<ResolverValue>.Constructor();
            OpEntry undecided = OpEntry.Empty;
            OpEntry entry;
            int pos = 0; // used and updated only for additional error checking. The algorithm does not depend on this.
            int level = 0;
            

            Match match = _regex.Match(query);
            while(match.Success) {
                if (pos != match.Index) return runner.Complete(ReportError("Syntax error at {0} - unrecognized text",match.Index));
                pos = match.Index + match.Length;
                if (match.Groups[0].Success) {
                    for (int i = 1; i < match.Groups.Count; i++) {
                        if (match.Groups[i].Success) {
                            string curval = match.Groups[i].Value;
                            switch ((Terms)i) {
                                case Terms.keyword:
                                    if (!undecided.IsEmpty) {
                                        return runner.Complete(ReportError("Syntax error at {0}.", match));
                                    }
                                    undecided = new OpEntry(curval, Terms.keyword, match.Index);
                                goto nextTerm;
                                case Terms.identifier:
                                    if (!undecided.IsEmpty) {
                                        return runner.Complete(ReportError("Syntax error at {0}.", match));
                                    }
                                    undecided = new OpEntry(curval,Terms.identifier,match.Index);
                                goto nextTerm;
                                case Terms.openbracket:
                                    if (undecided.Term == Terms.identifier) {
                                        opstack.Push(undecided); // Function call
                                        undecided = OpEntry.Empty;
                                    } else if (undecided.Term == Terms.keyword) {
                                        undecided.Addresses.Add(runner.Address);
                                        opstack.Push(undecided);
                                        undecided = OpEntry.Empty;
                                    } else if (undecided.IsEmpty) {
                                        opstack.Push(new OpEntry(null, Terms.compound,match.Index));
                                    }
                                    level ++;
                                goto nextTerm;
                                case Terms.closebracket:
                                    if (undecided.Term == Terms.identifier) {
                                        runner.Add(new Instruction(Instructions.PushParam,undecided.Value));
                                        undecided = OpEntry.Empty;
                                        AddArg(opstack, runner);
                                    }
                                    // *** Function call
                                    if (opstack.Count == 0) return runner.Complete(ReportError("Syntax error - function call has no function name at {0}",match));
                                    entry = opstack.Pop();
                                    if (entry.Term == Terms.identifier) {
                                        runner.Add(new Instruction(Instructions.Call, entry.Value,entry.Arguments));
                                        AddArg(opstack, runner);
                                    } else if (entry.Term == Terms.keyword) {
                                        AddArg(opstack, runner);
                                        // TODO: Operator completion

                                    } else {
                                        return runner.Complete(ReportError("Syntax error - function call has no function name at {0}",match));
                                    }
                                    level --;
                                goto nextTerm;
                                case Terms.comma:
                                    if (undecided.Term == Terms.identifier) {
                                        runner.Add(new Instruction(Instructions.PushParam, undecided.Value));
                                        undecided = OpEntry.Empty;
                                        AddArg(opstack, runner);
                                    } else if (!undecided.IsEmpty) { // If this happend it will be our mistake. Nothing but identifiers should appear in the undecided
                                        return runner.Complete(ReportError("Internal error at {0}",undecided.Pos));
                                    }
                                    // TODO: Consider root level behavior! Multiple results may be useful?
                                    if (opstack.Count == 0 || opstack.Peek().Term == Terms.compound) {
                                        // A coma in compond operator or on root level - only the last one must remain in the stack
                                        // For this reason we dump the last entry.
                                        
                                    }
                                goto nextTerm;
                                case Terms.numliteral:
                                    if (!undecided.IsEmpty) return runner.Complete(ReportError("Syntax error at {0}",undecided.Pos));
                                    if (curval.IndexOf('.') >= 0) { // double
                                        if (double.TryParse(curval,NumberStyles.Any,CultureInfo.InvariantCulture, out double t)) {
                                            runner.Add(new Instruction(Instructions.PushDouble, t));
                                            AddArg(opstack);
                                        } else {
                                            return runner.Complete(ReportError("Invalid double number at {0}",match));
                                        }
                                    } else {
                                        if (int.TryParse(curval,NumberStyles.Any,CultureInfo.InvariantCulture, out int n)) {
                                            runner.Add(new Instruction(Instructions.PushInt,n));
                                            AddArg(opstack);
                                        } else {
                                            return runner.Complete(ReportError("Invalid integer number at {0}",match));
                                        }
                                    }
                                goto nextTerm;
                                case Terms.specialliteral:
                                    if (!undecided.IsEmpty) return runner.Complete(ReportError("Syntax error at {0}",undecided.Pos));
                                    if (curval == "null") {
                                        runner.Add(new Instruction(Instructions.PushNull));
                                    } else if (curval == "true") {
                                        runner.Add(new Instruction(Instructions.PushBool,true));
                                    } else if (curval == "false") {
                                        runner.Add(new Instruction(Instructions.PushBool,false));
                                    } else {
                                        return runner.Complete(ReportError("Syntax error at {0}",match));
                                    }
                                    AddArg(opstack);
                                goto nextTerm;
                                case Terms.stringliteral:
                                    if (!undecided.IsEmpty) {
                                        return runner.Complete(ReportError("Syntax error at {0}", undecided.Pos));
                                    }
                                    runner.Add(new Instruction(Instructions.PushString,curval));
                                    AddArg(opstack);
                                goto nextTerm;
                                case Terms.space:
                                    // do nothing - we simply ignore the space
                                goto nextTerm;
                                case Terms.end:
                                    if (undecided.Term == Terms.identifier) {
                                        runner.Add(new Instruction(Instructions.PushParam, undecided.Value));
                                        undecided = OpEntry.Empty;
                                        AddArg(opstack);
                                    }
                                    if (opstack.Count == 0) {
                                        // The stack must be empty at this point
                                        return runner.Complete();
                                    } else {
                                        return runner.Complete("Syntax error at the expression end - check for matching brackets");
                                    }
                                // break;
                                default:
                                    return runner.Complete(ReportError("Syntax error at {0}",match));
                                
                            }
                        } // catch actual group
                    } // Check every possible group
                } else {
                    // Unrecognized or end
                }
                nextTerm:
                match = match.NextMatch();
            } // next term
            return runner.Complete("Parsing the query failed.");
        }
    }
}