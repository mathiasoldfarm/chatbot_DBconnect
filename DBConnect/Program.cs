using System;

namespace DBConnect
{
    class Program
    {
        static void Main(string[] args)
        {
            //Reader reader = new Reader();
            Writer writer = new Writer();
            TextWriter textWriter = new TextWriter();
            TextReader textReader = new TextReader();
            //DocsWriter docsWriter = new DocsWriter();

            //textReader.run();
            
            //DB TO TEXT
            //textWriter.run();

            //TEXT TO JSON
            textReader.run();

            //JSON TO DB
            writer.run();

            //docsWriter.run();
        }
    }
}
