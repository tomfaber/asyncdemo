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
    public class CallbackSeparationTest
    {
        ServiceTester tests = new ServiceTester((mat) => new CallbackSeparation(mat));

        [TestMethod]
        public void CallbackSeparationWhenDCalled()
        {
            tests.WhenDCalled();
        }

        [TestMethod]
        public void CallbackSeparationWhenDNotCalled()
        {
            tests.WhenDNotCalled();
        }

        [TestMethod]
        public void CallbackSeparationCallbackShouldBeCalled()
        {
            tests.CallbackShouldBeCalled();
        }

        [TestMethod]
        public void CallbackSeparationPreserveAsyncState()
        {
            tests.PreserveAsyncState();
        }
    }
}