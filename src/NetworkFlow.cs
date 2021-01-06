/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NetworkFlow
{
    class Program
    {
        #region Arc
        class Arc
        {
            public class Edge
            {
                public int from, to, capacity, flow;
                public Edge(int u, int v, int c, int f)
                {
                    from = u;
                    to = v;
                    capacity = c;
                    flow = f;
                }
            }

            public static void add_flow(int idx, int flow, ref List<Edge> edges)
            {
                edges[idx].flow += flow;
                edges[idx ^ 1].flow -= flow;
            }
            public static void add_edge(int u, int v, int c, ref List<Edge> edges, ref List<int>[] graph)
            {
                Edge from_edge = new Edge(u, v, c, 0);
                Edge to_edge = new Edge(v, u, 0, 0);
                graph[u].Add(edges.Count);
                edges.Add(from_edge);
                graph[v].Add(edges.Count);
                edges.Add(to_edge);
            }
        }
        #endregion

        #region Edmonds Karp
        class EdmondsKarp
        {


            static List<int>[] graph;
            static int[] parentsList;
            static List<Arc.Edge> edges = new List<Arc.Edge>();

            static void bfs(int startNode, int endNode)
            {

                parentsList = Enumerable.Repeat((int)-1, graph.Length).ToArray();

                Queue<int> q = new Queue<int>();
                q.Enqueue(startNode);

                while (q.Count != 0)
                {
                    int currentNode = q.Dequeue();

                    for (int i = 0; i < graph[currentNode].Count; i++)
                    {
                        int idx = graph[currentNode][i];
                        Arc.Edge e = edges[idx];
                        if (parentsList[e.to] == -1 && e.capacity > e.flow && e.to != startNode)
                        {
                            parentsList[e.to] = idx;
                            if (e.to == endNode)
                                return;
                            q.Enqueue(e.to);
                        }
                    }
                }
            }

            static int edmondsKarp(int startNode, int endNode)
            {
                int maxFlow = 0;
                while (true)
                {
                    bfs(startNode, endNode);
                    if (parentsList[endNode] == -1)
                    {
                        break;
                    }
                    int flow = int.MaxValue;
                    for (int node = parentsList[endNode]; node != -1; node = parentsList[edges[node].from])
                    {
                        flow = Math.Min(flow, edges[node].capacity - edges[node].flow);
                    }
                    maxFlow += flow;
                    int currentNode = parentsList[endNode];
                    while (currentNode != -1)
                    {
                        Arc.add_flow(currentNode, flow, ref edges);
                        currentNode = parentsList[edges[currentNode].from];
                    }
                }
                return maxFlow;
            }

            public static void Test(int NumTestCases)
            {
                string path_running_time = "running_time/EdmonsKarp.txt";
                List<string> running_times = new List<string>();
                for (int test_case = 1; test_case <= NumTestCases; ++test_case)
                {
                    string path = "tests/" + test_case.ToString() + ".txt";
                    FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(file);
                    string[] line = sr.ReadLine().Split(' ');
                    int num_of_nodes = int.Parse(line[0]);
                    int num_of_edges = int.Parse(line[1]);
                    line = sr.ReadLine().Split(' ');
                    int source = int.Parse(line[0]) - 1;
                    int sink = int.Parse(line[1]) - 1;
                    graph = new List<int>[num_of_nodes];
                    edges = new List<Arc.Edge>();
                    for (int j = 0; j < num_of_nodes; j++)
                    {
                        graph[j] = new List<int>();
                    }
                    for (int i = 0; i < num_of_edges; ++i)
                    {
                        line = sr.ReadLine().Split(' ');
                        int from = int.Parse(line[0]) - 1, to = int.Parse(line[1]) - 1, capacity = int.Parse(line[2]);
                        Arc.add_edge(from, to, capacity, ref edges, ref graph);
                    }
                    Stopwatch sw = Stopwatch.StartNew();
                    int maxFlow = edmondsKarp(source, sink);
                    sw.Stop();
                    int model_answer = int.Parse(sr.ReadLine().Split(' ')[0]);
                    if (maxFlow == model_answer)
                    {
                        Console.WriteLine("Test #" + test_case + " is sucessfull and took " + sw.ElapsedMilliseconds + " milliseconds");
                        running_times.Add(num_of_nodes + "," + num_of_edges + "," + maxFlow + "," + sw.ElapsedMilliseconds);
                    }
                    else
                    {
                        Console.WriteLine("Wrong answer on test #" + test_case + ", the right one is " + model_answer);
                    }
                    file.Close();
                }
                File.WriteAllLines(path_running_time, running_times);
            }
        }
        #endregion

        #region Ford Fulkerson
        class FordFloukerson
        {
            static List<int>[] graph = new List<int>[500];
            static List<Arc.Edge> edges;
            static int[] parent;
            static bool DFS(ref List<int>[] graph, int node, int sink, int source)
            {
                if (node == sink)
                    return true;

                foreach (var i in graph[node])
                {
                    if (parent[edges[i].to] == -1 && edges[i].capacity > edges[i].flow && edges[i].to != source)
                    {
                        parent[edges[i].to] = i;
                        if (DFS(ref graph, edges[i].to, sink, source)) return true;
                    }
                }
                return false;
            }

            static int Fordfulkerson(List<int>[] graph, int n, int s, int t)
            {

                int max_flow = 0;

                while (true)
                {
                    parent = new int[n];
                    for (int i = 0; i < n; ++i)
                        parent[i] = -1;
                    DFS(ref graph, s, t, s);
                    if (parent[t] == -1)
                        break;

                    int path_flow = int.MaxValue;

                    for (int node = parent[t]; node != -1; node = parent[edges[node].from])
                    {
                        path_flow = Math.Min(path_flow, edges[node].capacity - edges[node].flow);
                    }

                    for (int node = parent[t]; node != -1; node = parent[edges[node].from])
                    {
                        Arc.add_flow(node, path_flow, ref edges);
                    }
                    max_flow += path_flow;
                }

                return max_flow;
            }

            public static void Test(int NumTestCases)
            {
                string path_running_time = "running_time/FordFlukerson.txt";
                List<string> running_times = new List<string>();
                for (int test_case = 1; test_case <= NumTestCases; ++test_case)
                {
                    edges = new List<Arc.Edge>();
                    string path = "tests/" + test_case.ToString() + ".txt";
                    FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(file);
                    string[] line = sr.ReadLine().Split(" ");
                    int num_of_nodes = int.Parse(line[0]);
                    for (int k = 0; k < num_of_nodes; k++)
                    {
                        graph[k] = new List<int>();
                    }

                    int num_of_edges = int.Parse(line[1]);
                    line = sr.ReadLine().Split(" ");
                    int source = int.Parse(line[0]) - 1;
                    int sink = int.Parse(line[1]) - 1;
                    for (int i = 0; i < num_of_edges; ++i)
                    {
                        line = sr.ReadLine().Split(" ");
                        int u = int.Parse(line[0]) - 1, v = int.Parse(line[1]) - 1, c = int.Parse(line[2]);
                        Arc.add_edge(u, v, c, ref edges, ref graph);
                    }
                    int model_answer = int.Parse(sr.ReadLine().Split(" ")[0]);
                    Stopwatch sw = Stopwatch.StartNew();
                    int max_flow = Fordfulkerson(graph, num_of_nodes, source, sink);
                    sw.Stop();
                    if (max_flow == model_answer)
                    {
                        Console.WriteLine("Test #" + test_case + " is sucessfull and took " + sw.ElapsedMilliseconds + " milliseconds");
                        running_times.Add(num_of_nodes + "," + num_of_edges + "," + max_flow + "," + sw.ElapsedMilliseconds);
                    }
                    else
                    {
                        Console.WriteLine("Wrong answer on test #" + test_case + ", the right one is " + model_answer);
                        break;
                    }
                }
                File.WriteAllLines(path_running_time, running_times.ToArray());
            }
        }

        #endregion
        static void Main(string[] args)
        {
            Console.Write("Enter the number of test cases to run: ");
            int test = int.Parse(Console.ReadLine());
            Console.WriteLine("* * * * * * * * * * Edmonds Karp * * * * * * * * * *");
            EdmondsKarp.Test(test);
            Console.WriteLine("* * * * * * * * *  Ford Fulkerson  * * * * * * * * *");
            FordFloukerson.Test(test);
        }
    }
}