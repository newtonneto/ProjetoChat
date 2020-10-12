using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ChatCliente
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private string nomeUsuario = "Desconhecido";
        private StreamWriter stwEnviador;
        private StreamReader strReceptor;
        private TcpClient tcpServidor;
        //Atualiza o formulário com mensagens de outra thread
        private delegate void AtualizaLogCallBack(string strMensagem);
        //Define o formulpario para o estado "disconnected" de outra thread
        private delegate void FechaConexaoCallBack(string strMotivo);
        private Thread mensagemThread;
        private IPAddress enderecoIP;
        private bool conectado;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConectar_Click(object sender, RoutedEventArgs e)
        {
            if (conectado == false)
            {
                InicializaConexao();
            }
            else
            {
                FechaConexao("Desconectado a pedido do usuário");
            }
        }

        private void InicializaConexao()
        {
            //Trata o endereço IP informado em um objeto IPAddress
            enderecoIP = IPAddress.Parse(txtServidorIP.Text);
            //Inicia uma nova conexão TCP com o servidor
            tcpServidor = new TcpClient();
            tcpServidor.Connect(enderecoIP, 2502);

            //Altera o controlador de conexão para true
            conectado = true;

            //Prepara o formulário
            nomeUsuario = txtUsuario.Text;

            //Desabilita e habilita os campos apropriados
            txtServidorIP.IsEnabled = false;
            txtUsuario.IsEnabled = false;
            txtMensagem.IsEnabled = true;
            btnConectar.IsEnabled = true;
            btnEnviar.IsEnabled = true;
            btnConectar.Content = "Desconectar";

            //Envia o nome do usuário ao servidor
            stwEnviador = new StreamWriter(tcpServidor.GetStream());
            stwEnviador.WriteLine(txtUsuario.Text);
            stwEnviador.Flush();

            //Inicia a thread para receber mensagens e nova comunicação
            mensagemThread = new Thread(new ThreadStart(RecebeMensagens));
            mensagemThread.Start();
        }

        private void RecebeMensagens()
        {
            //Recebe a resposta do servidor
            strReceptor = new StreamReader(tcpServidor.GetStream());
            string ConResposta = strReceptor.ReadLine();
            
            //Se o primeiro caractere da resposta for 1 a conexão é feita com sucesso
            if (ConResposta[0] == '1')
            {
                //Atualiza o formulário para informar que esta conectado
                this.Dispatcher.Invoke(new AtualizaLogCallBack(this.AtualizaLog), new object[] { "Conectado" });
            }
            //Se o primeiro caractere não for 1 a conexão falhou
            else
            {
                string motivo = "Não conectado";
                //Extrai o motivo da mensagem resposta. O motivo começa no 3º caractere
                motivo += ConResposta.Substring(2, ConResposta.Length - 2);
                //Atualiza o formulário como o motivo da falha na conexão
                this.Dispatcher.Invoke(new FechaConexaoCallBack(this.FechaConexao), new object[] { motivo });
                //Sai do metodo
                return;
            }

            //Enquanto estiver conectado, le as linhas que estão chegando do servidor
            while (conectado)
            {
                try
                {
                    this.Dispatcher.Invoke(new AtualizaLogCallBack(this.AtualizaLog), new object[] { strReceptor.ReadLine() });
                }
                catch
                {

                }
            }
        }

        private void AtualizaLog(string strMensagem)
        {
            //Anexa texto ao final de cada linha
            txtLog.AppendText(strMensagem + "\r\n");
        }

        private void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            EnviaMensagem();
        }

        //private void txtMesagem_KeyDown(object sender, KeyEventArgs e)
        //{
            //Pressionar a tecla Enter para enviar a mensagem
            //if (e.Key == Key.Return)
            //{
                //EnviaMensagem();
            //}
        //}

        //Envia a mensagem para o servidor
        private void EnviaMensagem()
        {
            if (txtMensagem.LineCount >= 1)
            {
                //Envia a mensagem escrita pelo usuário
                stwEnviador.WriteLine(txtMensagem.Text);
                stwEnviador.Flush();
            }
            txtMensagem.Text = "";
        }

        private void FechaConexao(string motivo)
        {
            //Mostra o motivo de encerramento da conexão
            txtLog.AppendText(motivo + "\r\n");
            //Desabilita e habilita os campos apropriados
            txtServidorIP.IsEnabled = true;
            txtUsuario.IsEnabled = true;
            txtMensagem.IsEnabled = false;
            btnConectar.IsEnabled = false;
            btnConectar.Content = "Conectar";

            //Fecha os objetos
            conectado = false;
            stwEnviador.Close();
            strReceptor.Close();
            tcpServidor.Close();
        }
    }
}
