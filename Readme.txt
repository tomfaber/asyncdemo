.NET Async Demo

The scenario: imagine you are building a web service using WCF, need great scalability, and have decided
to implement your API using the Async pattern.  WCF makes it easy for you to expose your API as Async, just
provide Begin and End methods and use [OperationContractAttribute(AsyncPattern=true)].  See http://msdn.microsoft.com/en-us/library/ms731177.aspx
for details on this.

Things get more complicated once you realize that your service depends on calls to lots of other services -
you now need to make all of those calls asynchronous.  This is where you get a big payoff for all of this
work - if you were waiting 50ms for data to come back from the database, and 200ms each for responses from
external web services you're calling over HTTP, then previously, your threads were locked while you were
waiting for those responses.  Using Async will lower your latency on each user request, and allow you to serve
more requests in parallel.  But this is not easy!  You've heard that "Tasks" are the way to do Async in .NET, 
but when all of the APIs you're calling implement the APM with Begin/End methods, it's hard to turn those into
Tasks, and you wonder is it really worth it to sandwich one Async programming model inside another.

To demonstrate the multiple ways of implementing Async in .NET, I've constructed an interface (IWebService)
that you are implementing, and a mock class that simulates all of the external dependencies the service needs
to call.  The Mock exposes A, B, C, and D, which have to be
called in the following way:
	1) Call A first.
	2) you can do these in any order you like, or in parallel:
		Call B, passing in the result from A.
		Call C, passing in the result from A.
	3) If the result from B is greater than the result from C, call D passing in both results.
	4) Return the result from D (if called) or the result from C to the caller.

Complex, no?  And I haven't even specified how to handle errors or timeouts.

What you should look at:   I coded up four different ways of solving this problem.  Each one implements
the interface IPublicApi and consumes the APIs exposed by MockAsyncThing.   These are all in the Implementations
project.  To see them run, just run any of the tests in Implementations.UnitTests.

You should really look at the code, but the four patterns I used are:
	a) APM with anonymous delegates.
	b) APM with separate callback methods.
	c) Wrapping APM in tasks (requires .NET 4 or above)
	d) await/async in C# 5 (.NET 4.5)
	
Other possible implementations:
- AsyncEnumerator: this is in the Power Threading library you can download from Wintellect 
(http://wintellect.com/powerthreading.aspx). It is similar to the pattern in await/async, more options
but harder to use.
- await/async with a custom INotifyCompletion.  For an example, see http://blogs.msdn.com/b/pfxteam/archive/2012/01/23/10259822.aspx, 
or http://blogs.msdn.com/b/pfxteam/archive/2011/01/13/10115642.aspx.

I don't have time this week to do performance tests to see how each implementation compares in latency, throughput,
and resource utilization when run in WCF, but that would be a great exercise.

This is my first submission to github, I'd be thrilled to hear any suggestions/comments/flames you'd 
like to send my way.

Many thanks to the many blog posts, msdn articles, and github code I gained knowledge and inspiration from during the course of
writing this, especially the work of Jeffrey Richter and Stephen Toub.

   Copyright 2013 Tom Faber
