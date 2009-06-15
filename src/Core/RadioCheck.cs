#region WatiN Copyright (C) 2006-2009 Jeroen van Menen

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

using WatiN.Core.Logging;
using WatiN.Core.Native;

namespace WatiN.Core
{
	/// <summary>
	/// Base class for <see cref="CheckBox"/> and <see cref="RadioButton"/> provides
	/// support for common functionality.
	/// </summary>
    public class RadioCheck<E> : Element<E> where E : Element
	{
        public RadioCheck(DomContainer domContainer, INativeElement element) : base(domContainer, element) { }

        public RadioCheck(DomContainer domContainer, ElementFinder finder) : base(domContainer, finder) { }

        public virtual bool Checked
		{
			get { return bool.Parse(GetAttributeValue("checked")); }
			set
			{
				Logger.LogAction("Selecting " + GetType().Name + " '" + ToString() + "'");

				Highlight(true);
				SetAttributeValue("checked", value.ToString().ToLowerInvariant());
				FireEvent("onClick");
				Highlight(false);
			}
		}

		public override string ToString()
		{
			return Id;
		}
	}
}
