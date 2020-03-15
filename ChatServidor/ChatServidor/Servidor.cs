using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;

namespace ChatServidor
{
    //Trata os argumento para o evento StatusChanged
    public class StatusChangedEventArgs : EventArgs
    {
        //Mensagem que descreve o evento
        private string eventMsg;

        //Propriedade para retornar e definir uma mensagem do evento
        public string EventMessage
        {
            get { return eventMsg; }
            set { eventMsg = value; }
        }

        //Construtor para definir a mensagem do evento
        public StatusChangedEventArgs(string strEventMsg)
        {
            eventMsg = strEventMsg;
        }
    }

    //Delegate necessário para especificar os parametros que estamos passando para o evento
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    class Servidor
    {
        //Esta hashtable armazena os usuários e conexões (acessado/consultado por usuário)
        public static Hashtable htUsuarios = new Hashtable(30); //30 usuários é o limite de conexões
        //Esta hashtable armazena as conexões e os usuários (acessada/consultada por conexão)
        public static Hashtable htConexoes = new Hashtable(30); //30 conexões é o limite
        //Armazena o endereço IP
        private IPAddress enderecoIP;
        private TcpClient tcpCliente;
        //O evento e o seu argumento irá notificar o formulário quando um usuário se conecta, desconecta, envia uma mensagem e etc
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        //O construtor define o endereço IP para aquele retornado pela instanciação do objeto
        public Servidor(IPAddress endereco)
        {
            enderecoIP = endereco;
        }

        //A thread que ira tratar o escutador de conexões
        private Thread thrListener;

        //O objeto TCP object que escuta as conexões
        private TcpListener tlsCliente;

        //Ira dizer ao laço while para manter a monitoração das conexões
        bool servRodando = false;

        //Inclui o usuário nas hashtables
        public static void IncluiUsuario(TcpClient tcpUsuario, string strUsername)
        {
            //Primeiro inclui o nome e a conexão associada para ambas as hashtables
            Servidor.htUsuarios.Add(strUsername, tcpUsuario);
            Servidor.htConexoes.Add(tcpUsuario, strUsername);

            //Informa a nova conexão para todos os usuários e para o formulário do servidor
            EnviaMensagemAdmin(htConexoes[tcpUsuario] + " entrou...");
        }

        //Remove usuários das hashtables
        public static void RemoveUsuario(TcpClient tcpUsuario)
        {
            //Se o usuário existir
            if (htConexoes[tcpUsuario] != null)
            {
                //Primeiro mostra a informaçõa e informa os outros usuário sobre a conexão
                EnviaMensagemAdmin(htConexoes[tcpUsuario] + " saiu...");

                //Remove o usuário da hastable
                Servidor.htUsuarios.Remove(Servidor.htConexoes[tcpUsuario]);
                Servidor.htConexoes.Remove(tcpUsuario);
            }
        }

        //Este evento é chamado quando queremos disparar o evento StatusChanged
        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;
            if (statusHandler != null)
            {
                //Invoca o delegate
                statusHandler(null, e);
            }
        }

        //Envia mensagens administrativas
        public static void EnviaMensagemAdmin(string mensagem)
        {
            StreamWriter swSenderSender;

            //Exibe primeiro na aplicação
            e = new StatusChangedEventArgs("Administrador: " + mensagem);
            OnStatusChanged(e);

            //Cria um array de clientes TCPs do tamanho do numero de clientes existentes
            TcpClient[] tcpClientes = new TcpClient[Servidor.htUsuarios.Count];
            //Copia os objetos TcpClient no array
            Servidor.htUsuarios.Values.CopyTo(tcpClientes, 0);
            //Percorre a lista de clientes TCP
            for (int i = 0; i < tcpClientes.Length; i++)
            {
                //Tenta enviar uma mensagem para cada cliente
                try
                {
                    //Se a mensagem estiver em branco ou a conexão for nula, sai
                    if (mensagem.Trim() == "" || tcpClientes[i] == null)
                    {
                        continue;
                    }
                    //Envia a mensagem para o usuário atual no laço
                    swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                    swSenderSender.WriteLine("Administrador: " + mensagem);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                //Se houver um problema, o usuário não existe, então remove-o
                catch
                {
                    RemoveUsuario(tcpClientes[i]);
                }
            }
        }

        //Envia mensagens de um usário para todos os outros
        public static void EnviaMensagem(string origem, string mensagem)
        {
            StreamWriter swSenderSender;

            //Primeiro exibe a mensagem na aplicação
            e = new StatusChangedEventArgs(origem + " disse: " + mensagem);
            OnStatusChanged(e);

            //Cria um array de cliente TCPs do tamanho do número de clientes existentes
            TcpClient[] tcpClientes = new TcpClient[Servidor.htUsuarios.Count];
            //Copia os objetos TcpClient no array
            Servidor.htUsuarios.Values.CopyTo(tcpClientes, 0);
            //Percorre a lista de clientes TCP
            for (int i = 0; i < tcpClientes.Length; i++)
            {
                //Tenta enviar uma mensagem para cada cliente
                try
                {
                    //Se a mensagem estiver em branco ou a conexão for nula, sai
                    if (mensagem.Trim() == "" || tcpClientes[i] == null)
                    {
                        continue;
                    }
                    //Envia a mensagem para o usuário atual no laço
                    swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                    swSenderSender.WriteLine(origem + " disse: " + mensagem);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                //Se houver um problema, o usuário não existe, então remove-o
                catch
                {
                    RemoveUsuario(tcpClientes[i]);
                }
            }
        }

        public void IniciaAtendimento()
        {
            try
            {
                //Pega o IP do primeiro dispostivo da rede
                IPAddress ipaLocal = enderecoIP;

                //Cria um objeto TCP listener usando o IP do servidor e porta definidas
                tlsCliente = new TcpListener(ipaLocal, 2502);

                //Inicia o TCP listener e escuta as conexões
                tlsCliente.Start();

                //O laço While verifica se o servidor esta rodando antes de checar as conexões
                servRodando = true;

                //Inicia uma nova tread que hospeda o listener
                thrListener = new Thread(MantemAtendimento);
                thrListener.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void MantemAtendimento()
        {
            //Enquanto o servidor estiver rodando
            while (servRodando == true)
            {
                // Aceita uma conexão pendente
                tcpCliente = tlsCliente.AcceptTcpClient();
                // Cria uma nova instância da conexão
                Conexao newConnection = new Conexao(tcpCliente);
            }
        }
    }
}
