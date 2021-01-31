using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace Root_ProcessMediator
{
    public class PipeClient
    {
        public PipeClient(string name, string serverName)
        {
            var pipeClient = new NamedPipeClientStream(".", name, PipeDirection.Out, PipeOptions.None);

            try
            {
                Task.Run(() =>
                {
                    pipeClient.Connect();

                    var ss = new StreamString(pipeClient);

                    pipeClient.Close();
                });
            }
            catch(IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

    }
}
