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

            //reader.run();
            writer.run();
            //textWriter.run();
            //textReader.run();
            //docsWriter.run();
        }
    }
}
