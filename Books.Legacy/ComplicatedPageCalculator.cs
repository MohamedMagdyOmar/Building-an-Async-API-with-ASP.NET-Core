using System;
using System.Diagnostics;
using System.Threading;

namespace Books.Legacy
{
    public class ComplicatedPageCalculator 
    {
        /// <summary>
        /// Full CPU load for 5 seconds
        /// </summary>
        public int CalculateBookPages()
        {
            // this method simulates that we calculate number of pages in a book
            // this method is legacy "Non Async" code that is long running on the CPU Thread
            var watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                if (watch.ElapsedMilliseconds > 5000)
                {
                    break;
                } 
            }

            return 42;
        } 
    }
}
