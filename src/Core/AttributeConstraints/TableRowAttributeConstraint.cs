#region WatiN Copyright (C) 2006-2007 Jeroen van Menen

//Copyright 2006-2007 Jeroen van Menen
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

#endregion Copyright

namespace WatiN.Core
{
  using System;
  using System.Text.RegularExpressions;
  using mshtml;
  using WatiN.Core.Comparers;
  using WatiN.Core.Interfaces;

  /// <summary>
  /// Use this class to find a row which contains a particular value
  /// in a table cell contained in a table column.
  /// </summary>
  public class TableRowAttributeConstraint : AttributeConstraint
  {
    private int columnIndex;
    private ICompare containsText;
      
    /// <summary>
    /// Initializes a new instance of the <see cref="TableRowAttributeConstraint"/> class.
    /// </summary>
    /// <param name="findText">The text to find (exact match but case insensitive).</param>
    /// <param name="inColumn">The column index in which to look for the value.</param>
    public TableRowAttributeConstraint(string findText, int inColumn): base(Find.textAttribute, new StringEqualsAndCaseInsensitiveComparer(findText))
    {
      columnIndex = inColumn;
      containsText = new StringContainsAndCaseInsensitiveComparer(findText);
    }
      
    /// <summary>
    /// Initializes a new instance of the <see cref="TableRowAttributeConstraint"/> class.
    /// </summary>
    /// <param name="findTextRegex">The regular expression to match with.</param>
    /// <param name="inColumn">The column index in which to look for the value.</param>
    public TableRowAttributeConstraint(Regex findTextRegex, int inColumn): base(Find.textAttribute, findTextRegex)
    {
      columnIndex = inColumn;
      containsText = new AlwaysTrueComparer();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableRowAttributeConstraint"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    /// <param name="inColumn">The column index in which to look for the value.</param>
    public TableRowAttributeConstraint(ICompare comparer, int inColumn) : base(Find.textAttribute, comparer)
    {
      columnIndex = inColumn;
      containsText = new AlwaysTrueComparer();
    }
  
    public override bool Compare(IAttributeBag attributeBag)
    {
      IHTMLElement element = ((ElementAttributeBag)attributeBag).IHTMLElement;

      if (IsTextContainedIn(element.innerText))
      {
        // Get all elements and filter this for TableCells
        IHTMLElementCollection allElements = (IHTMLElementCollection)element.all;
        IHTMLElementCollection tableCellElements = (IHTMLElementCollection)allElements.tags(ElementsSupport.TableCellTagName);
        
        if (tableCellElements.length - 1 >= columnIndex)
        {
          IHTMLElement tableCell = (IHTMLElement)tableCellElements.item(columnIndex, null);
          return base.Compare(new ElementAttributeBag(tableCell));
        }
      }
        
      return false;
    }

    public bool IsTextContainedIn(string text)
    {
      return containsText.Compare(text);
    }
  }

  [Obsolete("Use TableRowAttributeConstraint instead")]
  public class TableRowFinder : AttributeConstraint
  {
    public TableRowFinder(string attributeName, ICompare comparer) : base(attributeName, comparer)
    {
    }

    public TableRowFinder(string attributeName, Regex regex) : base(attributeName, regex)
    {
    }

    public TableRowFinder(string attributeName, string value) : base(attributeName, value)
    {
    }
  }
}