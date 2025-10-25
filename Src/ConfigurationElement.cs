// Copyright (c) 2013-2025 Cem Dervis, MIT License.
// https://sharpconfig.org

using System;

namespace SharpConfig
{
  /// <summary>
  /// Represents the base class of all elements
  /// that exist in a <see cref="Configuration"/>,
  /// such as sections and settings.
  /// </summary>
  public abstract class ConfigurationElement
  {
    private static readonly string[] formattedPreCommentSeparator = new[] { "\r\n", "\n" };

    internal ConfigurationElement(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException(nameof(name));
      }

      Name = name;
    }

    /// <summary>
    /// Gets the name of this element.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the comment of this element.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Gets the comment above this element.
    /// </summary>
    public string PreComment { get; set; }

    /// <summary>
    /// Gets the string representation of the element.
    /// </summary>
    ///
    public override string ToString()
    {
      var stringExpr = GetStringExpression();

      if (Comment != null && PreComment != null && !Configuration.IgnoreInlineComments &&
          !Configuration.IgnorePreComments)
      {
        // Include inline comment and pre-comments.
        return $"{GetFormattedPreComment()}{Environment.NewLine}{stringExpr} {GetFormattedComment()}";
      }
      else if (Comment != null && !Configuration.IgnoreInlineComments)
      {
        // Include only the inline comment.
        return $"{stringExpr} {GetFormattedComment()}";
      }
      else if (PreComment != null && !Configuration.IgnorePreComments)
      {
        // Include only the pre-comments.
        return $"{GetFormattedPreComment()}{Environment.NewLine}{stringExpr}";
      }
      else
      {
        // In every other case, just return the expression.
        return stringExpr;
      }
    }

    // Gets a formatted comment string that is ready to be written to a config file.
    private string GetFormattedComment()
    {
      // Only get the first line of the inline comment.
      var comment = Comment;

      var newLineIndex = Comment.IndexOfAny(Environment.NewLine.ToCharArray());
      if (newLineIndex >= 0)
      {
        comment = comment.Substring(0, newLineIndex);
      }

      return Configuration.PreferredCommentChar + " " + comment;
    }

    // Gets a formatted pre-comment string that is ready
    // to be written to a config file.
    private string GetFormattedPreComment()
    {
      var lines = PreComment.Split(formattedPreCommentSeparator, StringSplitOptions.None);

      return string.Join(
          Environment.NewLine, Array.ConvertAll(lines, s => Configuration.PreferredCommentChar + " " + s));
    }

    /// <summary>
    /// Gets the element's expression as a string.
    /// An example for a section would be "[Section]".
    /// </summary>
    /// <returns>The element's expression as a string.</returns>
    protected abstract string GetStringExpression();
  }
}
