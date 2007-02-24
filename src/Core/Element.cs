#region WatiN Copyright (C) 2006-2007 Jeroen van Menen

// WatiN (Web Application Testing In dotNet)
// Copyright (C) 2006-2007 Jeroen van Menen
//
// This library is free software; you can redistribute it and/or modify it under the terms of the GNU 
// Lesser General Public License as published by the Free Software Foundation; either version 2.1 of 
// the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without 
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with this library; 
// if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 
// 02111-1307 USA 

#endregion Copyright

using System;
using System.Collections;
using System.Threading;

using mshtml;
using WatiN.Core.Exceptions;
using WatiN.Core.Logging;

namespace WatiN.Core
{
  using System.Reflection;

  /// <summary>
  /// This is the base class for all other element types in this project, like
  /// Button, Checkbox etc.. It provides common functionality to all these elements
  /// </summary>
  public class Element
  {
    private DomContainer domContainer;
    private object element;
    private ElementFinder elementFinder;
    
    private string originalcolor;

    /// <summary>
    /// This constructor is mainly used from within WatiN.
    /// </summary>
    /// <param name="domContainer"><see cref="DomContainer" /> this element is located in</param>
    /// <param name="element">The element</param>
    public Element(DomContainer domContainer, object element)
    {
      init(domContainer, element, null);
    }
    
    /// <summary>
    /// This constructor is mainly used from within WatiN.
    /// </summary>
    /// <param name="domContainer"><see cref="DomContainer"/> this element is located in</param>
    /// <param name="elementFinder">The element finder.</param>
    public Element(DomContainer domContainer, ElementFinder elementFinder)
    {
      init(domContainer, null, elementFinder);
    }
    
    /// <summary>
    /// This constructor is mainly used from within WatiN.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="elementTags">The element tags the element should match with.</param>
    public Element(Element element, ArrayList elementTags)
    {
      if (ElementTag.IsValidElement(element.htmlElement, elementTags))
      {
        init(element.domContainer, element.element, element.elementFinder);
      }
      else
      {
        throw new ArgumentException(String.Format("Expected element {0}", ElementFinder.GetExceptionMessage(elementTags)), "element");
      }
    }

    private void init(DomContainer domContainer, object element, ElementFinder elementFinder)
    {
      this.domContainer = domContainer;
      this.element = element;
      this.elementFinder = elementFinder;
    }

    /// <summary>
    /// Gets the name of the stylesheet class assigned to this ellement (if any).
    /// </summary>
    /// <value>The name of the class.</value>
    public string ClassName
    {
      get { return htmlElement.className; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Element"/> is completely loaded.
    /// </summary>
    /// <value><c>true</c> if complete; otherwise, <c>false</c>.</value>
    public bool Complete
    {
      get { return htmlElement2.readyStateValue == 4; }
    }
    
    /// <summary>
    /// Gets a value indicating whether this <see cref="Element"/> is enabled.
    /// </summary>
    /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
    public bool Enabled
    {
      get { return !htmlElement3.disabled; }
    }

    /// <summary>
    /// Gets the id of this element as specified in the HTML.
    /// </summary>
    /// <value>The id.</value>
    public string Id
    {
      get { return htmlElement.id; }
    }

    /// <summary>
    /// Gets the innertext of this element (and the innertext of all the elements contained
    /// in this element).
    /// </summary>
    /// <value>The innertext.</value>
    public virtual string Text
    {
      get { return htmlElement.innerText; }
    }

    /// <summary>
    /// Returns the text displayed after this element when it's wrapped
    /// in a Label element; otherwise it returns <c>null</c>.
    /// </summary>
    public string TextAfter
    {
      get { return htmlElement2.getAdjacentText("afterEnd"); }
    }

    /// <summary>
    /// Returns the text displayed before this element when it's wrapped
    /// in a Label element; otherwise it returns <c>null</c>.
    /// </summary>
    public string TextBefore
    {
      get { return htmlElement2.getAdjacentText("beforeBegin"); }
    }

    /// <summary>
    /// Gets the inner HTML of this element.
    /// </summary>
    /// <value>The inner HTML.</value>
    public string InnerHtml
    {
      get { return htmlElement.innerHTML; }
    }

    /// <summary>
    /// Gets the outer text.
    /// </summary>
    /// <value>The outer text.</value>
    public string OuterText
    {
      get { return htmlElement.outerText; }
    }

    /// <summary>
    /// Gets the outer HTML.
    /// </summary>
    /// <value>The outer HTML.</value>
    public string OuterHtml
    {
      get { return htmlElement.outerHTML; }
    }

    /// <summary>
    /// Gets the tag name of this element.
    /// </summary>
    /// <value>The name of the tag.</value>
    public string TagName
    {
      get { return htmlElement.tagName; }
    }

    /// <summary>
    /// Gets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title
    {
      get { return htmlElement.title; }
    }

    /// <summary>
    /// Gets the next sibling of this element in the Dom of the HTML page.
    /// </summary>
    /// <value>The next sibling.</value>
    public Element NextSibling
    {
      get
      {
        IHTMLElement nextSibling = domNode.nextSibling as IHTMLElement;
        if (nextSibling != null)
        {
          return GetTypedElement(domContainer, nextSibling);
        }
        return null;
      }
    }
    
    /// <summary>
    /// Gets the previous sibling of this element in the Dom of the HTML page.
    /// </summary>
    /// <value>The previous sibling.</value>
    public Element PreviousSibling
    {
      get
      {
        IHTMLElement previousSibling = domNode.previousSibling as IHTMLElement;
        if (previousSibling != null)
        {
          return GetTypedElement(domContainer, previousSibling);
        }
        return null;
      }
    }

    /// <summary>
    /// Gets the parent element of this element.
    /// If the parent type is known you can cast it to that type.
    /// </summary>
    /// <value>The parent.</value>
    /// <example>
    /// The following example shows you how to use the parent property.
    /// Assume your web page contains a bit of html that looks something
    /// like this:
    /// 
    /// div
    ///   a id="watinlink" href="http://watin.sourceforge.net" /
    ///   a href="http://sourceforge.net/projects/watin" /
    /// /div
    /// div
    ///   a id="watinfixturelink" href="http://watinfixture.sourceforge.net" /
    ///   a href="http://sourceforge.net/projects/watinfixture" /
    /// /div
    /// Now you want to click on the second link of the watin project. Using the 
    /// parent property the code to do this would be:
    /// 
    /// <code>
    /// Div watinDiv = (Div) ie.Link("watinlink").Parent;
    /// watinDiv.Links[1].Click();
    /// </code>
    /// </example>
    public Element Parent
    {
      get
      {
        IHTMLElement parentNode = domNode.parentNode as IHTMLElement;
        if (parentNode != null)
        {
          return GetTypedElement(domContainer, parentNode);
        }
        return null;
      }
    }

    public Style Style
    {
      get
      {
        return new Style(htmlElement.style);
      }
    }
    
    /// <summary>
    /// This methode can be used if the attribute isn't available as a property of
    /// Element or a subclass of Element.
    /// </summary>
    /// <param name="attributeName">The attribute name. This could be different then named in
    /// the HTML. It should be the name of the property exposed by IE on it's element object.</param>
    /// <returns>The value of the attribute if available; otherwise <c>null</c> is returned.</returns>
    public string GetAttributeValue(string attributeName)
    {
      if (UtilityClass.IsNullOrEmpty(attributeName))
      {
        throw new ArgumentNullException("attributeName", "Null or Empty not allowed.");
      }
      
      if (string.Compare(attributeName, "style", true) == 0 )
      {
        return htmlElement.style.cssText;
      }
      
      object attribute = htmlElement.getAttribute(attributeName, 0);

      if (attribute == DBNull.Value || attribute == null)
      {
        return null;
      }

      return attribute.ToString();
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString()
    {
      if (UtilityClass.IsNotNullOrEmpty(Title))
      {
        return Title;
      }
      return Text;
    }

    /// <summary>
    /// Clicks this element and waits till the event is completely finished (page is loaded 
    /// and ready) .
    /// </summary>
    public void Click()
    {
      if (!Enabled) { throw new ElementDisabledException(Id); }

      Logger.LogAction("Clicking " + GetType().Name + " '" + ToString() + "'");
      Highlight(true);

      ClickDispHtmlBaseElement();

      try
      {
        WaitForComplete();
      }
      finally
      {
        Highlight(false);
      }
    }

    private void ClickDispHtmlBaseElement()
    {
      DispHtmlBaseElement.click();
    }

    /// <summary>
    /// Clicks this instance and returns immediately. Use this method when you want to continue without waiting
    /// for the click event to be finished. Mostly used when a 
    /// HTMLDialog is displayed after clicking the element.
    /// </summary>
    public void ClickNoWait()
    {
      if (!Enabled) { throw new ElementDisabledException(Id); }

      Logger.LogAction("Clicking (no wait) " + GetType().Name + " '" + ToString() + "'");

      Highlight(true);

      Thread clickButton = new Thread(new ThreadStart(ClickDispHtmlBaseElement));
      clickButton.Start();
      clickButton.Join(500);

      Highlight(false);
    }

    /// <summary>
    /// Gives the (input) focus to this element.
    /// </summary>
    public void Focus()
    {
      if (!Enabled) { throw new ElementDisabledException(Id); }

      DispHtmlBaseElement.focus();
      FireEvent("onFocus");
    }

    /// <summary>
    /// Doubleclicks this element.
    /// </summary>
    public void DoubleClick()
    {
      if (!Enabled) { throw new ElementDisabledException(Id); }

      Logger.LogAction("Double clicking " + GetType().Name + " '" + ToString() + "'");

      FireEvent("onDblClick");
    }

    /// <summary>
    /// Does a keydown on this element.
    /// </summary>
    public void KeyDown()
    {
      FireEvent("onKeyDown");
    }

    /// <summary>
    /// Does a keyspress on this element.
    /// </summary>
    public void KeyPress()
    {
      FireEvent("onKeyPress");
    }

    /// <summary>
    /// Does a keyup on this element.
    /// </summary>
    public void KeyUp()
    {
      FireEvent("onKeyUp");
    }

    /// <summary>
    /// Fires the blur event on this element.
    /// </summary>
    public void Blur()
    {
      FireEvent("onBlur");
    }

    /// <summary>
    /// Fires the change event on this element.
    /// </summary>
    public void Change()
    {
      FireEvent("onChange");
    }

    /// <summary>
    /// Fires the mouseenter event on this element.
    /// </summary>
    public void MouseEnter()
    {
      FireEvent("onMouseEnter");
    }
    
    /// <summary>
    /// Fires the mousedown event on this element.
    /// </summary>
    public void MouseDown()
    {
      FireEvent("onmousedown");
    }

    /// <summary>
    /// Fires the mouseup event on this element.
    /// </summary>
    public void MouseUp()
    {
      FireEvent("onmouseup");
    }

    /// <summary>
    /// Fires the specified <paramref name="eventName"/> on this element
    /// and waits for it to complete.
    /// </summary>
    public void FireEvent(string eventName)
    {
      fireEvent(eventName, true);
    }

    /// <summary>
    /// Only fires the specified <paramref name="eventName"/> on this element.
    /// </summary>
    public void FireEventNoWait(string eventName)
    {
      fireEvent(eventName, false);
    }

    private void fireEvent(string eventName, bool waitForComplete)
    {
      if (!Enabled) { throw new ElementDisabledException(Id); }

      Highlight(true);
      domContainer.FireEvent(DispHtmlBaseElement, eventName);
      if (waitForComplete) {WaitForComplete();}
      Highlight(false);
    }

    /// <summary>
    /// Flashes this element 5 times.
    /// </summary>
    public void Flash()
    {
      Flash(5);
    }
    
    /// <summary>
    /// Flashes this element the specified number of flashes.
    /// </summary>
    /// <param name="numberOfFlashes">The number of flashes.</param>
    public void Flash(int numberOfFlashes)
    {
      for (int counter = 0; counter < numberOfFlashes; counter++)
      {
        Highlight(true);
        Thread.Sleep(250);
        Highlight(false);
        Thread.Sleep(250);
      }
    }

    /// <summary>
    /// Highlights this element.
    /// </summary>
    /// <param name="doHighlight">if set to <c>true</c> the element is highlighted; otherwise it's not.</param>
    protected void Highlight(bool doHighlight)
    {
      if (IE.Settings.HighLightElement)
      {
        if (doHighlight)
        {
          try
          {
            originalcolor = (string)htmlElement.style.backgroundColor;
            htmlElement.style.backgroundColor = IE.Settings.HighLightColor;
          }
          catch
          {
            originalcolor = null;
          }
        }
        else
        {
          try
          {
            if (originalcolor != null)
            {
              htmlElement.style.backgroundColor = originalcolor;
            }
            else
            {
              htmlElement.style.backgroundColor = "";
            }
          }
          catch {}
          finally
          {
            originalcolor = null;
          }
        }
      }
    }

    protected IHTMLElement htmlElement
    {
      get { return (IHTMLElement) HTMLElement; }
    }

    private IHTMLElement2 htmlElement2
    {
      get { return (IHTMLElement2) HTMLElement; }
    }

    private IHTMLElement3 htmlElement3
    {
      get { return (IHTMLElement3) HTMLElement; }
    }

    private IHTMLDOMNode domNode
    {
      get { return (IHTMLDOMNode) HTMLElement; }
    }

    /// <summary>
    /// Gets the DispHtmlBaseElement by casting <see cref="HTMLElement" />.
    /// </summary>
    /// <value>The DispHtmlBaseElement.</value>
    protected DispHTMLBaseElement DispHtmlBaseElement
    {
      get { return (DispHTMLBaseElement) HTMLElement; }
    }

    /// <summary>
    /// Gets the DOMcontainer for this element.
    /// </summary>
    /// <value>The DOM container.</value>
    public DomContainer DomContainer
    {
      get { return domContainer; }
    }

    /// <summary>
    /// Gets the DOM HTML element for this instance as an object. Cast it to 
    /// the interface you need. Most of the time the object supports IHTMLELement, 
    /// IHTMLElement2 and IHTMLElement3 but you can also cast it to a more
    /// specific interface. You should reference the microsoft.MSHTML.dll 
    /// assembly to cast it to a valid type.
    /// </summary>
    /// <value>The DOM element.</value>
    public object HTMLElement
    {
      get
      {
        if (element == null)
        {
          try
          {
            WaitUntilExists();
          }
          catch(WatiN.Core.Exceptions.TimeoutException)
          {}
        }
        
        return getElement(true);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Element"/> exists.
    /// </summary>
    /// <value><c>true</c> if exists; otherwise, <c>false</c>.</value>
    public bool Exists
    {
      get
      {
        object elementExists = getElement(false);

        if (elementExists == null)
        {
          return false;
        }

        try
        {
          if(((IHTMLElement)elementExists).sourceIndex < 0)
          {
            return false;
          }

          IHTMLElement htmlElementExists = elementExists as IHTMLElement;
          if(htmlElementExists != null && htmlElementExists.offsetParent == null)
//            && htmlElementExists.offsetHeight == 0 && htmlElementExists.offsetLeft == 0 
//            && htmlElementExists.offsetTop == 0 && htmlElementExists.offsetWidth == 0)
          {
            return false;
          }
        }
        catch
        {
          return false;
        }

        return true;
      }
    }

    /// <summary>
    /// Waits until the element exists or will time out after 30 seconds.
    /// To change the default time out, set <see cref="P:WatiN.Core.IE.Settings.WaitUntilExistsTimeOut"/>
    /// </summary>
    public void WaitUntilExists()
    {
      // Wait 30 seconds max
      WaitUntilExists(IE.Settings.WaitUntilExistsTimeOut);
    }
    
    /// <summary>
    /// Waits until the element exists. Wait will time out after <paramref name="timeout"/> seconds.
    /// </summary>
    /// <param name="timeout">The timeout in seconds.</param>
    public void WaitUntilExists(int timeout)
    {
      if (!Exists && elementFinder != null)
      {
        waitUntil(timeout, true);
      }      
    }

    /// <summary>
    /// Waits until the element no longer exists or will time out after 30 seconds.
    /// To change the default time out, set <see cref="P:WatiN.Core.IE.Settings.WaitUntilExistsTimeOut"/>
    /// </summary>
    public void WaitUntilRemoved()
    {
      // Wait 30 seconds max
      WaitUntilRemoved(IE.Settings.WaitUntilExistsTimeOut);
    }
    
    /// <summary>
    /// Waits until the element no longer exists. Wait will time out after <paramref name="timeout"/> seconds.
    /// </summary>
    /// <param name="timeout">The timeout in seconds.</param>
    public void WaitUntilRemoved(int timeout)
    {
      if (Exists)
      {
        waitUntil(timeout, false);
      }      
    }

    private void waitUntil(int timeout, bool waitUntilExists)
    {
      SimpleTimer timeoutTimer = new SimpleTimer(timeout);

      do
      {
        if (Exists == waitUntilExists)
        {
          return;
        }
        
        Thread.Sleep(200);
      } while (!timeoutTimer.Elapsed);

      throw new WatiN.Core.Exceptions.TimeoutException(string.Format("waiting {0} seconds for element to {1}.", timeout, waitUntilExists ? "show up" : "disappear"));
    }

    private object getElement(bool throwExceptionIfElementNotFound)
    {
      if (element == null && elementFinder != null)
      {
        element = elementFinder.FindFirst();

        if (element == null && throwExceptionIfElementNotFound)
        {
          throw elementFinder.CreateElementNotFoundException();
        }
      }

      return element;
    }

    /// <summary>
    /// Waits till the page load is complete. This should only be used on rare occasions
    /// because WatiN calls this function for you when it handles events (like Click etc..)
    /// To change the default time out, set <see cref="P:WatiN.Core.IE.Settings.WaitForCompleteTimeOut"/>
    /// </summary>
    public void WaitForComplete()
    {
      domContainer.WaitForComplete();
    }

    internal static Element GetTypedElement(DomContainer domContainer, IHTMLElement element)
    {
      Assembly assembly = Assembly.Load("WatiN.Core");

      Element returnElement = new ElementsContainer(domContainer, element);

      foreach (Type type in assembly.GetTypes())
      {
        if (type.IsSubclassOf(typeof(Element)))
        {
          PropertyInfo property = type.GetProperty("ElementTags");
          if (property != null)
          {
            ArrayList elementTags = (ArrayList)property.GetValue(type, null);

            if (ElementTag.IsValidElement(element, elementTags))
            {
              ConstructorInfo constructor = type.GetConstructor(new Type[] {typeof (Element)});
              if (constructor != null)
              {
                returnElement = (Element)constructor.Invoke(new object[] {returnElement});
                break;
              }
            }
          }
        }
      }

      return returnElement;
    }
  }
}
