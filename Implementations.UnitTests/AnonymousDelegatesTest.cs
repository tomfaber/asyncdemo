/* Copyright 2013 Tom Faber

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.  */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncDemo.Implementations.UnitTests
{
    [TestClass]
    public class AnonymousDelegatesTest
    {
        ServiceTester tests = new ServiceTester((mat) => new AnonymousDelegates(mat));

        [TestMethod]
        public void AnonymousDelegatesWhenDCalled()
        {
            tests.WhenDCalled();
        }

        [TestMethod]
        public void AnonymousDelegatesWhenDNotCalled()
        {
            tests.WhenDNotCalled();
        }

        [TestMethod]
        public void AnonymousDelegatesCallbackShouldBeCalled()
        {
            tests.CallbackShouldBeCalled();
        }

        [TestMethod]
        public void AnonymousDelegatesPreserveAsyncState()
        {
            tests.PreserveAsyncState();
        }
    }
}