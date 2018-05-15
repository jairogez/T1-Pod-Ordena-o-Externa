using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ordenacao_Externa
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Divide(".\\Arquivo.txt");
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("OPS...       Algo deu errado, lembre-se que o arquivo deve encontrar-se no mesmo diretório do executável e seu nome deve ser 'Arquivo.txt'. Verfifique também se o executável tem permissão para ler/escrever.");
                Console.ReadKey();
                return -1;
            }
            try
            {
                Classifica();
                Intercala();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
            return 0;
        }

        static void Divide(string arquivo)
        {
            int numParte = 1;
            StreamWriter sw = new StreamWriter(string.Format(".\\parte{0}.txt", numParte));
            using (StreamReader sr = new StreamReader(arquivo))
            {
                while (sr.Peek() >= 0)
                {
                    /* Escreve o registro */
                    sw.WriteLine(sr.ReadLine());

                    /* Se chegou ao limite de tamanho que impusemos, fecha e cria nova parte */
                    /* Ou se era a última linha simplesmente fecha */
                    if (sw.BaseStream.Length > 10000 && sr.Peek() >= 0)
                    {
                        sw.Close();
                        numParte++;
                        sw = new StreamWriter(string.Format(".\\parte{0}.txt", numParte));
                    }
                }
            }
            sw.Close();
        }

        static void Classifica()
        {
            foreach (string pedaco in Directory.GetFiles(".\\", "parte*.txt"))
            {
                /* coloca todos os registros dos pequenos arquivos, um por vez, dentro de um array e os ordena via mergeSort */
                string[] registros = File.ReadAllLines(pedaco);
                MergeSort(registros);
                /* cria o pedaço ordenado e deleta o antigo */
                string parteOrdenada = pedaco.Replace("parte", "parteOrdenada");
                File.WriteAllLines(parteOrdenada, registros);
                File.Delete(pedaco);
            }
        }

        /* Faz a intercalação entre os pedaços menores */
        static void Intercala()
        {
            string[] partes = Directory.GetFiles(".\\", "parteOrdenada*.txt");
            int qtdPedacos = partes.Length; /* número de pedaços em que o arquivo foi dividido */
            int tamanhoBuffer = 10; /* número de registros a serem guardados em cada fila(buffer) */

            /* abre os arquivos usando um streamReader que carrega um ponteiro para o inicio de cada arquivo */
            StreamReader[] readers = new StreamReader[qtdPedacos];
            for (int i = 0; i < qtdPedacos; i++)
                readers[i] = new StreamReader(partes[i]);

            /* monta as filas */
            Queue<string>[] filas = new Queue<string>[qtdPedacos];
            for (int i = 0; i < qtdPedacos; i++)
                filas[i] = new Queue<string>(tamanhoBuffer);

            /* carrega as filas */
            for (int i = 0; i < qtdPedacos; i++)
                CarregaFila(filas[i], readers[i], tamanhoBuffer);

            /* faz a intercalação propriamente dita */
            StreamWriter sw = new StreamWriter(".\\ArquivoOrdenado.txt");
            bool terminou = false;
            int menorIndice, j;
            string menorValor;
            while (!terminou)
            {
                /* encontra o pedaço com o menor valor */
                menorIndice = -1;
                menorValor = "";
                for (j = 0; j < qtdPedacos; j++)
                {
                    if (filas[j] != null)
                    {
                        if (menorIndice < 0 || String.CompareOrdinal(filas[j].Peek(), menorValor) < 0)
                        {
                            menorIndice = j;
                            menorValor = filas[j].Peek();
                        }
                    }
                }
                /* encerra se todas as filas estão vazias */
                if (menorIndice == -1) { terminou = true; break; }

                /* escreve o menor valor e o remove da fila */
                sw.WriteLine(menorValor);
                filas[menorIndice].Dequeue();

                /* se a fila ficou vazia, preenche, fazendo-a apontar para null quando já não houver registros para ler no seu respectivo arquivo */
                if (filas[menorIndice].Count == 0)
                {
                    CarregaFila(filas[menorIndice], readers[menorIndice], tamanhoBuffer);
                    if (filas[menorIndice].Count == 0)
                    {
                        filas[menorIndice] = null;
                    }
                }
            }
            sw.Close();

            /* fecha as partes(arquivos) e as deleta */
            for (int i = 0; i < qtdPedacos; i++)
            {
                readers[i].Close();
                File.Delete(partes[i]);
            }
        }

        static void CarregaFila(Queue<string> fila, StreamReader sr, int qtdRegistros)
        {
            for (int i = 0; i < qtdRegistros; i++)
            {
                if (sr.Peek() < 0) break;
                fila.Enqueue(sr.ReadLine());
            }
        }

        /* MergeSort usado para ordenar internamente os pedaços menores do arquivo */
        public static void MergeSort(string[] entrada)
        {
            MergeSort(entrada, 0, entrada.Length - 1);
        }

        /* Calcula o ponto médio e resolve os dois sub-problemas */
        public static void MergeSort(string[] entrada, int inicio, int fim)
        {
            if (inicio < fim)
            {
                int metade = (inicio / 2) + (fim / 2);
                MergeSort(entrada, inicio, metade);
                MergeSort(entrada, metade + 1, fim);
                Merge(entrada, inicio, metade, fim);
            }
        }

        /* Une os sub-arranjos em um único conjunto ordenado */
        private static void Merge(string[] entrada, int inicio, int meio, int fim)
        {
            int esquerda = inicio;
            int direita = meio + 1;
            string[] tmp = new string[(fim - inicio) + 1];
            int i = 0;

            while ((esquerda <= meio) && (direita <= fim))
            {
                if (String.CompareOrdinal(entrada[esquerda], entrada[direita]) < 0)
                {
                    tmp[i] = entrada[esquerda];
                    esquerda = esquerda + 1;
                }
                else
                {
                    tmp[i] = entrada[direita];
                    direita = direita + 1;
                }
                i = i + 1;
            }

            if (esquerda <= meio)
            {
                while (esquerda <= meio)
                {
                    tmp[i] = entrada[esquerda];
                    esquerda = esquerda + 1;
                    i = i + 1;
                }
            }

            if (direita <= fim)
            {
                while (direita <= fim)
                {
                    tmp[i] = entrada[direita];
                    direita = direita + 1;
                    i = i + 1;
                }
            }
            for (int j = 0; j < tmp.Length; j++)
            {
                entrada[inicio + j] = tmp[j];
            }
        }
    }
}
