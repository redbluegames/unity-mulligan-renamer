/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
namespace RedBlueGames.Tools
{
    using UnityEngine;
    using System.Collections;

    public class BulkRenamer
    {
        private const string AddedTextColorTag = "<color=green>";
        private const string DeletedTextColorTag = "<color=red>";
        private const string EndColorTag = "</color>";

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public string SearchToken { get; set; }

        public string ReplacementString { get; set; }

        public string RichTextPrefix
        {
            get
            {
                return string.Concat(AddedTextColorTag, this.Prefix, EndColorTag);
            }
        }

        public string RichTextSuffix
        {
            get
            {
                return string.Concat(AddedTextColorTag, this.Suffix, EndColorTag);
            }
        }

        public string RichTextReplacementString
        {
            get
            {
                return string.Concat(
                    DeletedTextColorTag,
                    this.SearchToken,
                    EndColorTag,
                    AddedTextColorTag,
                    this.ReplacementString,
                    EndColorTag);
            }
        }

        public BulkRenamer()
        {
            this.Prefix = string.Empty;
            this.Suffix = string.Empty;
            this.SearchToken = string.Empty;
            this.ReplacementString = string.Empty;
        }

        public string GetRenamedString(string originalName, bool useRichText)
        {
            var modifiedName = originalName;

            // Replace strings first so we don't replace the prefix.
            if (!string.IsNullOrEmpty(this.SearchToken))
            {
                var replacementString = useRichText ? this.RichTextReplacementString :
                this.ReplacementString;
                modifiedName = modifiedName.Replace(this.SearchToken, replacementString);
            }

            if (!string.IsNullOrEmpty(this.Prefix))
            {
                var prefix = useRichText ? this.RichTextPrefix : this.Prefix;
                modifiedName = string.Concat(prefix, modifiedName);
            }

            if (!string.IsNullOrEmpty(this.Suffix))
            {
                var suffix = useRichText ? this.RichTextSuffix : this.Suffix;
                modifiedName = string.Concat(modifiedName, suffix);
            }

            return modifiedName;
        }
    }
}