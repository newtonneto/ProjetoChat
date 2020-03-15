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
        private TcpClient tcpClient;
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
        private TcpListener tlsClientes;

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
            }
        }
    }
}
