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

namespace ChatServidor
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void AtualizaStatusCallback(string strMensagem);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnAtender_Click(object sender, RoutedEventArgs e)
        {
            //Analisa o endereço IP do servidor informado no txtIP
            IPAddress enderecoIP = IPAddress.Parse(txtIP.Text);

            //Cria uma nova instância do objeto ChatServidor
            Servidor mainServidor = new Servidor(enderecoIP);

            //Vincula o tratamento de evento StatusChanged a mainServer_StatusChanged
            //Servidor.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged); #########################ARRUMAR#########################

            //Inicia o atendimento das conexões
            mainServidor.IniciaAtendimento();

            //Mostra que nos iniciamos o atendimento para conexões
            txtLog.AppendText("Monitorando as conexões...\r\n");
        }
        
        public void mainServidor_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            //Chama o método que atualiza o formulário
            this.Dispatcher.Invoke(new AtualizaStatusCallback(this.AtualizaStatus), new object[] { e.EventMessage });
        }

        private void AtualizaStatus(string strMensagem)
        {
            //Atualiza o log com mensagens
            txtLog.AppendText(strMensagem + "\r\n");
        }
    }
}
