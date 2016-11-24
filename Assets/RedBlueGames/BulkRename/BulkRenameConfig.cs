using UnityEngine;
using System.Collections;

public class BulkRenameConfig
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

    public BulkRenameConfig()
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
