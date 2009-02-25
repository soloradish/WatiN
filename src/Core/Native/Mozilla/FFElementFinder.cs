﻿#region WatiN Copyright (C) 2006-2009 Jeroen van Menen

//Copyright 2006-2009 Jeroen van Menen
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

using System;
using System.Collections.Generic;
using WatiN.Core.Constraints;
using WatiN.Core.Interfaces;

namespace WatiN.Core.Native.Mozilla
{
    internal sealed class FFElementFinder : NativeElementFinder
    {
        private readonly FireFoxClientPort _clientPort;

        public FFElementFinder(IList<ElementTag> elementTags, BaseConstraint constraint, IElementCollection elementCollection, DomContainer domContainer, FireFoxClientPort clientPort)
            : base(elementTags, constraint, elementCollection, domContainer)
        {
            _clientPort = clientPort;
        }

        /// <inheritdoc />
        protected override ElementFinder FilterImpl(BaseConstraint findBy)
        {
            return new FFElementFinder(ElementTags, Constraint & findBy, ElementCollection, DomContainer, _clientPort);
        }

        /// <inheritdoc />
        protected override IEnumerable<Element> FindElementsByTag(string tagName)
        {
            // In case of a redirect this call makes sure the doc variable is pointing to the "active" page.
            _clientPort.InitializeDocument();

            if (ElementCollection.Elements != null)
            {
                var elementToSearchFrom = ElementCollection.Elements.ToString();
                var ElementArrayName = FireFoxClientPort.CreateVariableName();

                var numberOfElements = GetNumberOfElementsWithMatchingTagName(ElementArrayName, elementToSearchFrom,
                    tagName);

                for (var index = 0; index < numberOfElements; index++)
                {
                    var indexedElementVariableName = string.Format("{0}[{1}]", ElementArrayName, index);
                    var ffElement = new FFElement(indexedElementVariableName, _clientPort);

                    var element = WrapElementIfMatch(ffElement);
                    if (element == null) continue;
                    
                    AssignPersistentElementReference(ffElement);
                    yield return element;
                }
                DeReferenceElementArrayName(ElementArrayName);
            }
        }

        private void DeReferenceElementArrayName(string elementName)
        {
            var command = string.Format("{0} = null; ", elementName);

            _clientPort.Write(command);
        }

        /// <inheritdoc />
        protected override IEnumerable<Element> FindElementsById(string id)
        {
            // In case of a redirect this call makes sure the doc variable is pointing to the "active" page.
            _clientPort.InitializeDocument();

            if (ElementCollection.Elements == null) yield break;

            var referencedElement = ElementCollection.Elements.ToString();

            // Create reference to document object
            var documentReference = GetDocumentReference(referencedElement);

            var elementName = FireFoxClientPort.CreateVariableName();
            var command = string.Format("{0} = {1}.getElementById(\"{2}\"); ", elementName, documentReference, id);
            command = command + String.Format("{0} != null;", elementName);

            if (!_clientPort.WriteAndReadAsBool(command)) yield break;

            var ffElement = new FFElement(elementName, _clientPort);
            var element = WrapElementIfMatch(ffElement);
            
            if (element != null) yield return element;
        }

        private static string GetDocumentReference(string referencedElement)
        {
            if (referencedElement.Contains(".") && 
                !(referencedElement.EndsWith("contentDocument") || referencedElement.EndsWith("ownerDocument")))
            {
                return referencedElement + ".ownerDocument";
            }
            
            return referencedElement;
        }

        private void AssignPersistentElementReference(FFElement nativeElement)
        {
            var elementVariableName = FireFoxClientPort.CreateVariableName();
            _clientPort.Write("{0}={1};", elementVariableName, nativeElement.Object);

            nativeElement.ReAssignElementReference(elementVariableName);
        }

        private int GetNumberOfElementsWithMatchingTagName(string elementArrayName, string elementToSearchFrom, string tagName)
        {
            var tagToFind = string.IsNullOrEmpty(tagName) ? "*" : tagName;
            var command = string.Format("{0} = {1}.getElementsByTagName(\"{2}\"); ", elementArrayName, elementToSearchFrom, tagToFind);
            command = command + string.Format("{0}.length;", elementArrayName);

            return _clientPort.WriteAndReadAsInt(command);
        }
    }
}