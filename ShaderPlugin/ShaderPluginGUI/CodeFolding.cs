using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace ShaderPluginGUI
{
    public class GLSLFoldingStrategy
    {
        const string BRACE_FOLDING_NAME = "{ ... }";
        Regex AnyValidSymbol = new Regex("[a-zA-Z_]([a-zA-Z_]|[0-9])*");
        Regex IfDef = new Regex(@"#(if|ifdef|ifndef)\s+");
        Regex EndIf = new Regex(@"#(endif)\s*?$", RegexOptions.Multiline);
        Regex Region = new Regex(@"//region\s+", RegexOptions.Multiline);
        Regex EndRegion = new Regex(@"//(endregion)\s*?$", RegexOptions.Multiline);

        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            try
            {
                manager.UpdateFoldings(CreateFoldings(document), -1);
            }
            catch { }
        }

        enum FoldingTokenType
        {
            Unknown,
            Whitespace,
            NewLine,
            ParenthesisLefl,
            ParenthesisRight,
            BraceLeft,
            BraceRight,
            SingleLineComment,
            MultiLineCommentOpen,
            MultiLineCommentClose,
            IfDef,
            EndIf,
            Region,
            Endregion
        }

        private FoldingTokenType GetToken(string text, ref int pos, out int tokenID, out bool eof)
        {
            char current = text[pos];
            char next = pos == text.Length - 1 ? '\0' : text[pos + 1];
            if (next == '\0')
                eof = true;
            else
                eof = false;

            tokenID = pos;

            if (prevToken == FoldingTokenType.NewLine)
            {
                var regionMatch = Region.Match(text, pos, Math.Min(text.Length - pos, 9));
                if (regionMatch.Success && regionMatch.Index == pos)
                {
                    tokenID = regionMatch.Index + regionMatch.Length;
                    return FoldingTokenType.Region;
                }
                else
                {
                    var endRegionMatch = EndRegion.Match(text, pos/*, Math.Min(text.Length - pos, 11)*/);
                    if (endRegionMatch.Success && endRegionMatch.Index == pos)
                    {
                        tokenID = endRegionMatch.Groups[1].Index + endRegionMatch.Groups[1].Length;
                        return FoldingTokenType.Endregion;
                    }
                }
            }


            if (current == '/' && next == '/' && prevToken == FoldingTokenType.NewLine) return FoldingTokenType.SingleLineComment;
            if (current == '/' && next == '*') return FoldingTokenType.MultiLineCommentOpen;
            if (current == '*' && next == '/') return FoldingTokenType.MultiLineCommentClose;
            if (current == '\n') return FoldingTokenType.NewLine;
            if (current == '{') return FoldingTokenType.BraceLeft;
            if (current == '}') return FoldingTokenType.BraceRight;
            if (current == '(') return FoldingTokenType.ParenthesisLefl;
            if (current == ')') return FoldingTokenType.ParenthesisRight;

            if (prevToken == FoldingTokenType.NewLine)
            {
                var ifDefMatch = IfDef.Match(text, pos, Math.Min(text.Length - pos, 8));
                if (ifDefMatch.Success && ifDefMatch.Index == pos)
                {
                    tokenID = ifDefMatch.Index + ifDefMatch.Length;
                    return FoldingTokenType.IfDef;
                }
                else
                {
                    var endifMatch = EndIf.Match(text, pos/*, Math.Min(text.Length - pos, 8)*/);
                    if (endifMatch.Success && endifMatch.Index == pos)
                    {
                        tokenID = endifMatch.Groups[1].Index + endifMatch.Groups[1].Length;
                        return FoldingTokenType.EndIf;
                    }
                }
            }



            if (char.IsWhiteSpace(current)) return FoldingTokenType.Whitespace;
            return FoldingTokenType.Unknown;
        }

        enum FoldingState
        {
            Basic,
            MultiLineComment,
            SingleLineComment
        }

        FoldingTokenType prevToken;
        List<NewFolding> CreateFoldings(ITextSource document)
        {
            List<NewFolding> Foldings = new List<NewFolding>();
            Stack<int> stack = new Stack<int>();

            FoldingState currentState = FoldingState.Basic;
            prevToken = FoldingTokenType.NewLine;
            NewFolding singleLineFolding = null;
            int multiSingleFolding = 0;
            bool foldUnclosedMultiline = false;
            bool foldMultilineSections = false;
            FoldingTokenType singleCommentPrevToken = FoldingTokenType.Whitespace;
            for (int charIndex = 0; charIndex < document.TextLength; charIndex++)
            {
                FoldingTokenType token = GetToken(document.Text, ref charIndex, out int tokenIndex, out bool eof);

                switch (currentState)
                {
                    case FoldingState.MultiLineComment:
                        if (token == FoldingTokenType.NewLine)
                            foldUnclosedMultiline = true;

                        if (token == FoldingTokenType.MultiLineCommentClose)
                        {
                            currentState = FoldingState.Basic;
                            var foldingStart = stack.Pop();
                            if (foldUnclosedMultiline == true)
                            {
                                foldUnclosedMultiline = false;
                                string name = GetFoldingName(document, foldingStart);
                                Foldings.Add(new NewFolding(foldingStart, charIndex + 2) { Name = name });
                            }
                        }

                        if (eof)
                        {
                            if (stack.Count > 0)
                            {
                                var foldingStart = stack.Pop();

                                if (foldUnclosedMultiline)
                                {
                                    foldUnclosedMultiline = false;
                                    string name = GetFoldingName(document, foldingStart);
                                    Foldings.Add(new NewFolding(foldingStart, document.TextLength) { Name = name });
                                }
                            }
                            currentState = FoldingState.Basic;
                        }
                        break;
                    case FoldingState.SingleLineComment:
                        if (token == FoldingTokenType.NewLine)
                        {
                            currentState = FoldingState.Basic;
                            singleCommentPrevToken = FoldingTokenType.SingleLineComment;
                            var foldingStart = stack.Pop();
                            string name = GetFoldingName(document, foldingStart);
                            if (singleLineFolding == null)
                                singleLineFolding = new NewFolding(foldingStart, charIndex + 1) { Name = name };
                            else
                            {
                                if (charIndex > 0 && document.Text[charIndex - 1] == '\r')
                                    singleLineFolding.EndOffset = charIndex - 1;
                                else
                                    singleLineFolding.EndOffset = charIndex;
                            }
                        }
                        if (eof && singleLineFolding != null)
                        {
                            currentState = FoldingState.Basic;
                            if (stack.Count > 0)
                            {
                                var mathc = stack.Pop();
                                singleLineFolding.EndOffset = document.TextLength;
                            }
                            if (multiSingleFolding > 1)
                                Foldings.Add(singleLineFolding);
                        }
                        break;
                    default:
                        switch (token)
                        {
                            case FoldingTokenType.IfDef:
                                stack.Push(charIndex);
                                break;
                            case FoldingTokenType.EndIf:
                                if (stack.Count > 0)
                                {
                                    var foldingStartPreProc = stack.Pop();
                                    string name = GetFoldingName(document, foldingStartPreProc);
                                    Foldings.Add(new NewFolding(foldingStartPreProc, tokenIndex) { Name = name });
                                }
                                break;
                            case FoldingTokenType.Region:
                                stack.Push(charIndex);
                                break;
                            case FoldingTokenType.Endregion:
                                if (stack.Count > 0)
                                {
                                    var foldingStartRegion = stack.Pop();
                                    string name = GetFoldingName(document, foldingStartRegion);
                                    Foldings.Add(new NewFolding(foldingStartRegion, tokenIndex) { Name = name });
                                }
                                break;
                            case FoldingTokenType.BraceLeft:
                                int pos = charIndex;
                                for (int j = charIndex - 1; j >= 0; j--)
                                {
                                    char inner = document.Text[j];
                                    if (inner == '{' || inner == '}' || inner == ';' || inner == '(')
                                        break;
                                    if (inner == ')' || AnyValidSymbol.IsMatch(inner.ToString()))
                                    {
                                        bool isPostProcessor = false;
                                        for (int n = j; n >= 0; n--)
                                        {
                                            if (char.IsWhiteSpace(document.Text[n]))
                                                break;
                                            if (document.Text[n] == '#')
                                            {
                                                isPostProcessor = true;
                                                break;
                                            }
                                        }
                                        if (!isPostProcessor)
                                            pos = j + 1;
                                        break;
                                    }
                                }
                                foldMultilineSections = false;
                                stack.Push(pos);
                                break;
                            case FoldingTokenType.BraceRight:
                                if (stack.Count <= 0) continue;// return Foldings;//у нас не совпадают скобки
                                var foldingStart = stack.Pop();
                                if (foldMultilineSections)
                                    Foldings.Add(new NewFolding(foldingStart, charIndex + 1) { Name = BRACE_FOLDING_NAME });
                                foldMultilineSections = false;
                                break;
                            case FoldingTokenType.SingleLineComment:
                                stack.Push(tokenIndex); 
                                if (singleCommentPrevToken == FoldingTokenType.Whitespace)
                                {
                                    singleLineFolding = null;
                                    multiSingleFolding = 0;
                                }
                                currentState = FoldingState.SingleLineComment;
                                multiSingleFolding++;
                                break;
                            case FoldingTokenType.MultiLineCommentOpen:
                                stack.Push(charIndex);
                                currentState = FoldingState.MultiLineComment;
                                break;
                            case FoldingTokenType.MultiLineCommentClose:
                                break;
                            case FoldingTokenType.NewLine:
                                singleCommentPrevToken = FoldingTokenType.Whitespace;
                                foldMultilineSections = true;
                                CloseMultiSingleLine(Foldings, singleLineFolding, ref multiSingleFolding);
                                break;
                            case FoldingTokenType.Whitespace://нужно для правильной работы фолдинга однострочно/многостроного комментария
                                break;
                            default:
                                CloseMultiSingleLine(Foldings, singleLineFolding, ref multiSingleFolding);
                                break;
                        }
                        break;
                }
                if (token != FoldingTokenType.Whitespace)
                    prevToken = token;

            }

            // Close last Multiline Comment
            CloseMultiSingleLine(Foldings, singleLineFolding, ref multiSingleFolding);
            /*if (stack.Count > 0) // расскоментить если хотим закрывать вне незавершенные фолдинки по концу документа
                foreach (var item in stack)
                    Foldings.Add(new NewFolding(item.start, document.TextLength) { Name = "{ ... }" });*/
            if (Foldings.Count > 0)
                Foldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return Foldings;
        }

        private string GetFoldingName(ITextSource document, int startIndex)
        {
            int newLineIndex = document.Text.IndexOf('\n', startIndex);
            string name = BRACE_FOLDING_NAME;
            if (newLineIndex != -1)
                name = document.Text.Substring(startIndex, newLineIndex - startIndex).TrimEnd() + " ...";
            return name;
        }

        private void CloseMultiSingleLine(List<NewFolding> Foldings, NewFolding singleLineFolding, ref int multiSingleFolding)
        {
            if (singleLineFolding != null && multiSingleFolding > 1)
                Foldings.Add(singleLineFolding);
            singleLineFolding = null;
            multiSingleFolding = 0;
        }
    }

    public static class FoldingsHelper
    {
        public static void TryCollapsePhotoshopUniformsFoldings(FoldingManager FManager)
        {
            foreach (FoldingSection folding in FManager.AllFoldings)
            {
                if (!folding.IsFolded && folding.Title.TrimStart().StartsWith("//region Photoshop Uniforms"))
                {
                    folding.IsFolded = true;
                    return;
                }
            }
        }
    }
}