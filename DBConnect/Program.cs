using System;

namespace DBConnect
{
    class Program
    {
        static void Main(string[] args)
        {
            Reader reader = new Reader();
            Writer writer = new Writer();

            //reader.run();
            writer.run();
        }
    }
}
