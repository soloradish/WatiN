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

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using mshtml;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core.Constraints;
using WatiN.Core.DialogHandlers;
using WatiN.Core.Exceptions;
using WatiN.Core.Logging;
using WatiN.Core.UtilityClasses;

namespace WatiN.Core.UnitTests.IETests
{
    [TestFixture]
    public class IeTests : BaseWatiNTest
    {
        [TestFixtureSetUp]
        public override void FixtureSetup()
        {
            base.FixtureSetup();
            Logger.LogWriter = new DebugLogWriter();
        }

        [SetUp]
        public void SetUp()
        {
            Settings.Reset();
        }

        [Test]
        public void TestRunnerApartmentStateMustBeSTA()
        {
            Assert.IsTrue(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);
        }

        [Test]
        public void CheckIEIsInheritingProperTypes()
        {
            using (var ie = new IE())
            {
                Assert.IsInstanceOfType(typeof (IE), ie, "Should be an IE instance");
                Assert.IsInstanceOfType(typeof (Browser), ie, "Should be a Browser instance");
                Assert.IsInstanceOfType(typeof (DomContainer), ie, "Should be a DomContainer instance");
            }
        }

        [Test, Category("InternetConnectionNeeded")]
        public void GoogleSearchWithEncodedQueryStringInConstructor()
        {
            var url = string.Format("http://www.google.com/search?q={0}", HttpUtility.UrlEncode("a+b"));

            using (var ie = new IE(url))
            {
                Assert.That(ie.TextField(Find.ByName("q")).Value, Is.EqualTo("a+b"));
            }
        }

        [Test, Ignore("Second assert fails in nunit console mode.")]
        public void PressTabAndActiveElement()
        {
            using (var ie = new IE(MainURI))
            {
                ie.TextField("name").Focus();

                var element = ie.ActiveElement;
                Assert.AreEqual("name", element.Id);

                ie.PressTab();

                element = ie.ActiveElement;
                Assert.AreEqual("popupid", element.Id);
            }
        }

        [Test, Category("InternetConnectionNeeded")]
        public void AddProtocolToUrlIfOmmitted()
        {
            using (var ie = new IE("www.google.com"))
            {
                Assert.That(ie.Url, Text.StartsWith("http://"));
            }
        }

        [Test]
        public void NewIeInstanceShouldTake_Settings_MakeNewIeInstanceVisible_IntoAccount()
        {
            Settings.MakeNewIeInstanceVisible = true;
            Assert.That(Settings.MakeNewIeInstanceVisible, "Default should be true");

            using (var ie = new IE())
            {
                Assert.That(((SHDocVw.InternetExplorer) ie.InternetExplorer).Visible, "IE Should be visible");
            }

            Settings.MakeNewIeInstanceVisible = false;
            Assert.That(Settings.MakeNewIeInstanceVisible, Is.EqualTo(false), "should be false");

            using (var ie = new IE())
            {
                Assert.That(((SHDocVw.InternetExplorer) ie.InternetExplorer).Visible, Is.EqualTo(false), "IE Should be visible");
            }
        }

        // TODO: move to BrowserTests when HTMLDialogs are supported by FireFox
        [Test]
        public void DocumentShouldBeDisposedSoHTMLDialogGetsDisposedAndReferenceCountIsOK()
        {
            DialogWatcher dialogWatcher;
            int ReferenceCount;

            using (var ie = new IE(MainURI))
            {
                ReferenceCount = ie.DialogWatcher.ReferenceCount;

                ie.Button("popupid").Click();

                using (Document document = ie.HtmlDialog(Find.ByIndex(0)))
                {
                    Assert.AreEqual(ReferenceCount + 1, ie.DialogWatcher.ReferenceCount, "DialogWatcher reference count");
                }

                dialogWatcher = ie.DialogWatcher;
            }

            Assert.AreEqual(ReferenceCount - 1, dialogWatcher.ReferenceCount, "DialogWatcher reference count should be zero after test");
        }

        [Test]
        public void NewIEWithUrlAndLogonDialogHandler()
        {
            FailIfIEWindowExists("main", "NewIEWithUrlAndLogonDialogHandler");

            var url = MainURI.AbsoluteUri;
            var logon = new LogonDialogHandler("y", "z");

            using (var ie = new IE(url, logon))
            {
                Assert.AreEqual(MainURI, new Uri(ie.Url));
                Assert.IsTrue(ie.DialogWatcher.Contains(logon));
                Assert.AreEqual(1, ie.DialogWatcher.Count);

                using (var ie1 = new IE(url, logon))
                {
                    Assert.AreEqual(MainURI, new Uri(ie1.Url));
                    Assert.IsTrue(ie.DialogWatcher.Contains(logon));
                    Assert.AreEqual(1, ie.DialogWatcher.Count);
                }
            }

            Assert.IsFalse(IsIEWindowOpen("main"), "Internet Explorer should be closed by IE.Dispose");
        }

        [Test]
        public void GoToUri()
        {
            using (var ie = new IE())
            {
                ie.GoTo(MainURI);
                Assert.AreEqual(MainURI, new Uri(ie.Url));
            }
        }

        [Test]
        public void NewIEInSameProcess()
        {
            using (var ie1 = new IE())
            {
                using (var ie2 = new IE())
                {
                    Assert.AreEqual(ie1.ProcessID, ie2.ProcessID);
                }
            }
        }

        [Test]
        public void NewIEInNewProcess()
        {
            using (var ie1 = new IE())
            {
                using (var ie2 = new IE(true))
                {
                    Assert.IsNotNull(ie2, "create ie in new process returned null");
                    Assert.AreNotEqual(ie1.ProcessID, ie2.ProcessID, "process id problem");
                }
            }
        }

        [Test]
        public void NewIE()
        {
            using (var ie = new IE())
            {
                Assert.AreEqual("about:blank", ie.Url);
            }
        }

        [Test]
        public void NewIEWithUri()
        {
            using (var ie = new IE(MainURI))
            {
                Assert.AreEqual(MainURI, new Uri(ie.Url));
            }
        }

        [Test]
        public void NewIEWithUriShouldAutoClose()
        {
            FailIfIEWindowExists("main", "NewIEWithUriShouldAutoClose");

            using (new IE(MainURI)) {}

            Assert.IsFalse(IsIEWindowOpen("main"), "Internet Explorer should be closed by IE.Dispose");
        }

        [Test]
        public void NewIEWithUriNotAutoClose()
        {
            FailIfIEWindowExists("main", "NewIEWithUriNotAutoClose");

            using (var ie = new IE(MainURI))
            {
                Assert.IsTrue(ie.AutoClose);
                ie.AutoClose = false;
            }

            Assert.IsTrue(IsIEWindowOpen("main"), "Internet Explorer should NOT be closed by IE.Dispose");

            IE.AttachToIE(Find.ByTitle("main"), 3).Close();
        }

        [Test]
        public void NewIEWithUrl()
        {
            FailIfIEWindowExists("main", "NewIEWithUrl");

            var url = MainURI.AbsoluteUri;

            using (var ie = new IE(url))
            {
                Assert.AreEqual(MainURI, new Uri(ie.Url));
                Assert.AreEqual(0, ie.DialogWatcher.Count, "DialogWatcher count should be zero");
            }

            Assert.IsFalse(IsIEWindowOpen("main"), "Internet Explorer should be closed by IE.Dispose");
        }

        [Test]
        public void Reopen()
        {
            FailIfIEWindowExists("main", "Reopen");

            var url = MainURI.AbsoluteUri;
            using (var ie = new IE(url))
            {
                Assert.AreEqual(MainURI, new Uri(ie.Url));
                var oldIEObj = ie.InternetExplorer;

                ie.Reopen();
                Assert.AreNotSame(oldIEObj, ie.InternetExplorer, "Reopen should create a new browser.");

                Assert.AreEqual("about:blank", ie.Url);
            }
        }

        [Test]
        public void ReopenWithUrlAndLogonDialogHandlerInNewProcess()
        {
            FailIfIEWindowExists("main", "ReopenWithUrlAndLogonDialogHandler");

            var logon = new LogonDialogHandler("y", "z");

            using (var ie1 = new IE())
            {
                using (var ie2 = new IE())
                {
                    Assert.AreEqual(ie1.ProcessID, ie2.ProcessID, "process id problem");

                    Assert.AreEqual("about:blank", ie2.Url);
                    var oldIEObj = ie2.InternetExplorer;

                    ie2.Reopen(MainURI, logon, true);
                    Assert.AreNotSame(oldIEObj, ie2.InternetExplorer, "Reopen should create a new browser.");

                    Assert.AreNotEqual(ie1.ProcessID, ie2.ProcessID, "process id problem");

                    Assert.AreEqual(MainURI, new Uri(ie2.Url));
                    Assert.IsTrue(ie2.DialogWatcher.Contains(logon));
                    Assert.AreEqual(1, ie2.DialogWatcher.Count);
                }
            }

            Assert.IsFalse(IsIEWindowOpen("main"), "Internet Explorer should be closed by IE.Dispose");
        }

        [Test]
        public void RefreshWithNeverExpiredPage()
        {
            using (var ie = new IE(MainURI))
            {
                ie.TextField("name").TypeText("refresh test");

                ie.Refresh();

                Assert.AreEqual("refresh test", ie.TextField("name").Text);
            }
        }

        [Test]
        public void Cookies()
        {
            using (var ie = new IE())
            {
                // Clear all cookies.
                ie.ClearCookies();

                // Ensure our test cookies don't exist from a previous run.
                Assert.IsNull(ie.GetCookie("http://1.watin.com/", "test-cookie"));
                Assert.IsNull(ie.GetCookie("http://2.watin.com/", "test-cookie"));

                // Create cookies for a pair of domains.
                ie.SetCookie("http://1.watin.com/", "test-cookie=abc; expires=Wed, 01-Jan-2020 00:00:00 GMT");
                Assert.AreEqual("test-cookie=abc", ie.GetCookie("http://1.watin.com/", "test-cookie"));

                ie.SetCookie("http://2.watin.com/", "test-cookie=def; expires=Wed, 01-Jan-2020 00:00:00 GMT");
                Assert.AreEqual("test-cookie=def", ie.GetCookie("http://2.watin.com/", "test-cookie"));

                // Clear cookies under one subdomain.
                ie.ClearCookies("http://1.watin.com/");

                // Ensure just the cookie of the first subdomain was deleted.
                Assert.IsNull(ie.GetCookie("http://1.watin.com/", "test-cookie"));
                Assert.AreEqual("test-cookie=def", ie.GetCookie("http://2.watin.com/", "test-cookie"));

                // Clear cookies under master domain.
                ie.ClearCookies("http://watin.com/");

                // Ensure the second subdomain's cookie was deleted this time.
                Assert.IsNull(ie.GetCookie("http://2.watin.com/", "test-cookie"));
            }
        }

        [Test, Ignore("Experiencing problems. Test fails and following tests also fail. Need to look into ti")]
        public void ClearCache()
        {
            using (var ie = new IE(GoogleUrl))
            {
                // Testing cache clearing directly is a little difficult because we cannot
                // easily enumerate its contents without using system APIs.
                // We could create a sample page that includes a nonce but says it's cacheable
                // forever.  Then we should only observe the change on refresh or cache clearing.
                // Fortunately Google has already done it for us.

                // Save the original page html.
                var oldHtml = GetHtmlSource(ie);

                // If we navigate to the page again, we should see identical Html.
                ie.GoTo(GoogleUrl);
                Assert.AreEqual(oldHtml, GetHtmlSource(ie), "HTML should be identical when pulling from the cache.");

                // But after clearing the cache, things are different.
                ie.ClearCache();
                ie.GoTo(GoogleUrl);
                Assert.AreNotEqual(oldHtml, GetHtmlSource(ie), "HTML should differ after cache has been cleared.");
            }
        }

        private static string GetHtmlSource(Document ie)
        {
            var document = (IHTMLDocument2) ie.NativeDocument.Object;
            return document.body.parentElement.outerHTML;
        }

        [Test, Category("InternetConnectionNeeded")]
        public void RefreshWithImmediatelyExpiredPage()
        {
            using (var ie = new IE(GoogleUrl))
            {
                ie.TextField(Find.ByName("q")).TypeText("refresh test");

                ie.Refresh();

                Assert.AreEqual(null, ie.TextField(Find.ByName("q")).Text);
            }
        }

        [Test]
        public void BackAndForward()
        {
            using (var ie = new IE())
            {
                ie.GoTo(MainURI);
                Assert.AreEqual(MainURI, new Uri(ie.Url));

                ie.Link(Find.ByUrl(IndexURI)).Click();
                Assert.AreEqual(IndexURI, new Uri(ie.Url));

                var wentBack = ie.Back();
                Assert.AreEqual(MainURI, new Uri(ie.Url));
                Assert.That(wentBack, "Expected went back");

                var wentFoward = ie.Forward();
                Assert.AreEqual(IndexURI, new Uri(ie.Url));
                Assert.That(wentFoward, "Expected went forward");
            }
        }

        [Test]
        public void BackShouldIgnoreExceptionsWhenNoHistoryIsAvailable()
        {
            using (var ie = new IE())
            {
                try
                {
                    var wentBack = ie.Back();
                    Assert.That(wentBack, Is.False, "Expected no navigation back");
                }
                catch (Exception e)
                {
                    Assert.Fail("Shouldn't throw exception: " + e);
                }
            }
        }

        [Test]
        public void ForwardShouldIgnoreExceptionsWhenNoHistoryIsAvailable()
        {
            using (var ie = new IE())
            {
                try
                {
                    var wentForward = ie.Forward();
                    Assert.That(wentForward, Is.False, "Expected no navigation forward");
                }
                catch (Exception e)
                {
                    Assert.Fail("Shouldn't throw exception: " + e);
                }
            }
        }

        [Test]
        public void IEExistsByHWND()
        {
            var hwnd = "0";
            Assert.IsFalse(IE.Exists(Find.By("hwnd", hwnd)), "hwnd = 0 should not be found");

            using (var ie = new IE(MainURI))
            {
                hwnd = ie.hWnd.ToString();
                Assert.IsTrue(IE.Exists(Find.By("hwnd", hwnd)), "hwnd of ie instance should be found");
            }

            Assert.IsFalse(IE.Exists(Find.By("hwnd", hwnd)), "hwnd of closed ie instance should not be found");
        }

        [Test]
        public void IEExistsByUrl()
        {
            IEExistsAsserts(Find.ByUrl(MainURI));
        }

        [Test]
        public void IEExistsByTitle()
        {
            IEExistsAsserts(Find.ByTitle("Ai"));
        }

        private static void IEExistsAsserts(BaseConstraint findByUrl)
        {
            Assert.IsFalse(IE.Exists(findByUrl));

            using (new IE(MainURI))
            {
                Assert.IsTrue(IE.Exists(findByUrl));
            }

            Assert.IsFalse(IE.Exists(findByUrl));
        }

        /// <summary>
        /// Attaches to IE with a zero timeout interval. Allthough the timeout
        /// interval is zero the existing IE instance should be found.
        /// </summary>
        [Test]
        public void AttachToIEWithZeroTimeout()
        {
            // Create a new IE instance so we can find it.
            using (new IE(MainURI))
            {
                var startTime = DateTime.Now;
                using (IE.AttachToIE(Find.ByUrl(MainURI), 0))
                {
                    // Should return (within 1 second).
                    Assert.Greater(1, DateTime.Now.Subtract(startTime).TotalSeconds);
                }
            }
        }

        [Test, ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void AttachToIEWithNegativeTimeoutNotAllowed()
        {
            IE.AttachToIE(Find.ByTitle("Bogus title"), -1);
        }

        [Test]
        public void AttachToIEByPartialTitle()
        {
            FailIfIEWindowExists("Ai", "AttachToIEByPartialTitle");

            using (new IE(MainURI))
            {
                using (var ieMainByTitle = IE.AttachToIE(Find.ByTitle("Ai")))
                {
                    Assert.AreEqual(MainURI, ieMainByTitle.Uri);
                }
            }
        }

        [Test]
        public void AttachToIEByUrl()
        {
            FailIfIEWindowExists("Ai", "AttachToIEByUrl");

            using (new IE(MainURI))
            {
                using (var ieMainByUri = IE.AttachToIE(Find.ByUrl(MainURI)))
                {
                    Assert.AreEqual(MainURI, ieMainByUri.Uri);
                }
            }
        }

        [Test]
        public void NewIEClosedByDispose()
        {
            FailIfIEWindowExists("main", "IEClosedByDispose");

            using (new IE(MainURI))
            {
                using (var ie = IE.AttachToIE(Find.ByTitle("main")))
                {
                    Assert.AreEqual(MainURI, new Uri(ie.Url));
                }
            }

            Assert.IsFalse(IsIEWindowOpen("main"), "Internet Explorer not closed by IE.Dispose");
        }

        [Test]
        public void IENotFoundException()
        {
            var startTime = DateTime.Now;
            const int timeoutTime = 5;
            const string ieTitle = "Non Existing IE Title";
            const string expectedMessage = "Could not find an IE window matching constraint: Attribute 'title' with value 'non existing ie title'. Search expired after '5' seconds.";

            try
            {
                // Time out after timeoutTime seconds
                startTime = DateTime.Now;
                using (IE.AttachToIE(Find.ByTitle(ieTitle), timeoutTime)) {}

                Assert.Fail(string.Format("Internet Explorer with title '{0}' should not be found", ieTitle));
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof (IENotFoundException), e);
                // add 1 second to give it some slack.
                Assert.Greater(timeoutTime + 1, DateTime.Now.Subtract(startTime).TotalSeconds);
                Console.WriteLine(e.Message);
                Assert.AreEqual(expectedMessage, e.Message, "Unexpected exception message");
            }
        }


        [Test]
        public void NewUriAboutBlank()
        {
            Assert.AreEqual("about:blank", AboutBlank.AbsoluteUri);
        }

        [Test]
        public void CallingIEDisposeAfterIECloseShouldNotThrowAnExeption()
        {
            var ie = new IE();
            ie.Close();
            ie.Dispose();
        }

        [Test, ExpectedException(typeof (ObjectDisposedException))]
        public void CallingIEForceCloseAfterIECloseShouldThrowAnExeption()
        {
            var ie = new IE();
            ie.Close();
            ie.ForceClose();
        }

        /// <summary>
        /// This test addresses a problem in the IE collection where Windows Explorer
        /// windows may get included in the list of shell windows which is a bit
        /// problematic for methods like ForceClose since the test will timeout.
        /// </summary>
        [Test]
        public void IECollectionExcludesWindowsExplorerWindows()
        {
            // Bring up an Explorer window and wait for it to become visible.
            var info = new ProcessStartInfo("explorer.exe") {UseShellExecute = false, CreateNoWindow = true};

            var p = Process.Start(info);

            try
            {
                Thread.Sleep(2000);

                // Create an IE window so we know there's at least one of them.
                new IE();

                // Iterate over internet explorer windows.
                // Previously this would pick up a reference to the Windows Explorer
                // window which would timeout while waiting for the main document to
                // become available.
                Assert.GreaterOrEqual(IE.InternetExplorers().Length, 1);

                foreach (var ie in IE.InternetExplorers())
                    ie.Close();
            }
            finally
            {
                if (p != null)
                    if (! p.HasExited)
                        p.Kill();
            }
        }

        private static void FailIfIEWindowExists(string partialTitle, string testName)
        {
            if (IsIEWindowOpen(partialTitle))
            {
                Assert.Fail(string.Format("An Internet Explorer with '{0}' in it's title already exists. Test '{1}' can't be correctly tested. Close all IE windows and run this test again.", partialTitle, testName));
            }
        }

        private static bool IsIEWindowOpen(string partialTitle)
        {
            // Give windows some time to do work before checking
            Thread.Sleep(1000);

            return IE.Exists(Find.ByTitle(partialTitle));
        }

        [Test]
        public void TestWatiNWithInjectedHTMLCode()
        {
            var html = "<HTML><input name=txtSomething><input type=button name=btnSomething value=Click></HTML>";

            using(var ie = new IE())
            {
                var document = (IHTMLDocument2) ie.NativeDocument.Object;
                document.writeln(html);

                Assert.That(ie.Button(Find.ByName("btnSomething")).Exists);
            }
        }
    }

    [TestFixture]
    public class NewAttachToIeImplementation
    {

        [Test]
        public void Test()
        {
            ItterateProcesses();
        }

        public void ItterateProcesses()
        {
            var hWnd = IntPtr.Zero;

            var processes = Process.GetProcesses();
            Console.WriteLine("#processes = " + processes.Length);

            foreach (var process in processes)
            {

                foreach (ProcessThread t in process.Threads)
                {
                    var threadId = t.Id;

                    NativeMethods.EnumThreadProc callbackProc = EnumChildForTridentDialogFrame;
                    NativeMethods.EnumThreadWindows(threadId, callbackProc, hWnd);
                }
            }
        }

        private static bool EnumChildForTridentDialogFrame(IntPtr hWnd, IntPtr lParam)
        {
            if (IsIEFrame(hWnd))
            {
                Console.WriteLine("Is IE Window: " + ((long)hWnd).ToString("X"));

                Console.WriteLine(IEDOMFromhWnd(hWnd).title);
            }

            return false;
        }

        internal static IHTMLDocument2 IEDOMFromhWnd(IntPtr hWnd)
        {
            var IID_IHTMLDocument2 = new Guid("626FC520-A41E-11CF-A731-00A0C9082637");

            var lRes = 0;

            //if (IsIETridentDlgFrame(hWnd))
            //{

            while (!IsIEServerWindow(hWnd))
            {
                // Get 1st child IE server window
                hWnd = NativeMethods.GetChildWindowHwnd(hWnd, "Internet Explorer_Server");
            }

            if (IsIEServerWindow(hWnd))
            {
                // Register the message
                var lMsg = NativeMethods.RegisterWindowMessage("WM_HTML_GETOBJECT");

                // Get the object
                NativeMethods.SendMessageTimeout(hWnd, lMsg, 0, 0, NativeMethods.SMTO_ABORTIFHUNG, 1000, ref lRes);

                if (lRes != 0)
                {
                    // Get the object from lRes
                    IHTMLDocument2 ieDOMFromhWnd = null;

                    var hr = NativeMethods.ObjectFromLresult(lRes, ref IID_IHTMLDocument2, 0, ref ieDOMFromhWnd);

                    if (hr != 0)
                    {
                        throw new COMException("ObjectFromLresult has thrown an exception", hr);
                    }

                    return ieDOMFromhWnd;
                }
            }

            return null;
        }

        internal static bool IsIETridentDlgFrame(IntPtr hWnd)
        {
            return UtilityClass.CompareClassNames(hWnd, "Internet Explorer_TridentDlgFrame");
        }

        internal static bool IsIEFrame(IntPtr hWnd)
        {
            return UtilityClass.CompareClassNames(hWnd, "IEFrame");
        }

        private static bool IsIEServerWindow(IntPtr hWnd)
        {
            return UtilityClass.CompareClassNames(hWnd, "Internet Explorer_Server");
        }

    }
}