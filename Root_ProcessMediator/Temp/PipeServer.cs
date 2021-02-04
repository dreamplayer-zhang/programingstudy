using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_ProcessMediator
{
    public class PipeServer
    {
        private int maxNumberOfServerInstance = 4;

        public PipeServer(string name)
        {
            

            try
            {
                Task.Run(() =>
                {
                    NamedPipeServerStream pipeServer = new NamedPipeServerStream(name, PipeDirection.InOut, maxNumberOfServerInstance);

                    pipeServer.WaitForConnection();

                    StreamString ss = new StreamString(pipeServer);

                    ss.WriteString("I am the one true server");
                    string filename = ss.ReadString();
                });
                //pipeServer.RunAsClient(fileReader.Start);
            }

            catch(IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //pipeServer.Close();
            }
        }
    }
}
