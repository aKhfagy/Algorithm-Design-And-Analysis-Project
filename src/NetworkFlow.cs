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

        #region Dinic
        class TestDinic
        {
            class Dinic
            {
                //O(v *blocking_flow RT)
                //blocking_flow_RT --> DFS which O(VE)
                //DINIC COM = O(V^2 *E)

                List<List<Edge>> G;
                int V;
                List<int> level;
                List<int> iter;

                public Dinic(int vertex_size)
                {
                    V = vertex_size;
                    // initialize lists
                    G = new List<List<Edge>>();
                    for (int i = 0; i < V; ++i)
                    {
                        G.Add(new List<Edge>());
                    }
                    level = new List<int>();
                    iter = new List<int>();
                }

                class Edge
                {
                    public int To, Cap, Rev;
                    public Edge(int to, int cap, int rev) //rev is index of opposite edge.
                    {
                        To = to;
                        Cap = cap;
                        Rev = rev;
                    }

                }

                public void AddEdge(int from, int to, int cap)
                {
                    G[from].Add(new Edge(to, cap, G[to].Count)); // G[to].Count is index of reverse edge in to list
                    G[to].Add(new Edge(from, 0, G[from].Count - 1)); // G[from].Count - 1 is index of reverse edge in from list
                }

                public int MaxFlow(int s, int t)
                {
                    int flow = 0;
                    while (true) // O(V**2 * E)
                    {
                        BFS(s); // O(E)
                        if (level[t] < 0)
                        {
                            return flow;
                        }
                        for (int i = 0; i < V; ++i)
                        {
                            iter[i] = 0;
                        }
                        var f = DFS(s, t, int.MaxValue); // O(V + E)
                        while (f > 0) // O(VE)
                        {
                            flow += f;
                            f = DFS(s, t, int.MaxValue); // O(V + E)
                        }
                    }
                }
                // BFS 1) levels, 2) find path from s->t until blocking flow are reached
                void BFS(int s)     // O(E)
                {
                    for (int i = 0; i < V; ++i) 
                    {
                        level[i] = -1;
                    }
                    level[s] = 0;
                    var que = new Queue<int>();
                    que.Enqueue(s);

                    while (que.Count != 0)
                    {
                        var v = que.Dequeue();
                        for (int i = 0; i < G[v].Count; i++)
                        {
                            var e = G[v][i];
                            if (e.Cap > 0 && level[e.To] < 0)
                            {
                                level[e.To] = level[v] + 1;
                                que.Enqueue(e.To);
                            }

                        }

                    }

                }

                int DFS(int v, int t, int f)     //O(V + E)
                {
                    if (v == t) return f;
                    for (int i = iter[v]; i < G[v].Count; i++)
                    {
                        iter[v] = i;
                        var e = G[v][i];
                        if (e.Cap > 0 && level[v] < level[e.To])
                        {
                            var d = DFS(e.To, t, Math.Min(f, e.Cap));
                            if (d > 0)
                            {
                                e.Cap -= d;
                                G[e.To][e.Rev].Cap += d;
                                return d;
                            }
                        }
                    }
                    return 0;
                }
            }
            public static void Test(int NumTestCases)
            {
                string path_running_time = "running_time/Dinc.txt";
                List<string> running_times = new List<string>();
                for (int test_case = 1; test_case <= NumTestCases; ++test_case)
                {
                    string path = "tests/" + test_case.ToString() + ".txt";
                    FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(file);
                    string[] line = sr.ReadLine().Split(" ");
                    int num_of_nodes = int.Parse(line[0]);
                    int num_of_edges = int.Parse(line[1]);
                    Dinic dinic = new Dinic(num_of_nodes);
                    line = sr.ReadLine().Split(" ");
                    int source = int.Parse(line[0]) - 1;
                    int sink = int.Parse(line[1]) - 1;
                    for (int i = 0; i < num_of_edges; ++i)
                    {
                        line = sr.ReadLine().Split(" ");
                        int u = int.Parse(line[0]) - 1, v = int.Parse(line[1]) - 1, c = int.Parse(line[2]);
                        dinic.AddEdge(u, v, c);
                    }
                    int model_answer = int.Parse(sr.ReadLine().Split(" ")[0]);
                    Stopwatch sw = Stopwatch.StartNew();
                    int max_flow = dinic.MaxFlow(source, sink);
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
                // fill parent list from graph list ( carry list of array of neighbours of each node

                parentsList = Enumerable.Repeat((int)-1, graph.Length).ToArray(); // initialize all parentlist with -1 

                Queue<int> q = new Queue<int>(); // queue to sort each node with neighbours from start to end
                q.Enqueue(startNode); 

                while (q.Count != 0) // loop on our queue
                {
                    int currentNode = q.Dequeue(); // Take element of first place and dequeu it

                    for (int i = 0; i < graph[currentNode].Count; i++) // loop on array of list at each index 
                    {
                        int idx = graph[currentNode][i]; //index variable carry each element at he graph from zero index till count
                        Arc.Edge e = edges[idx];  // ARC.EDGE : nested class carry : from, to, capacity, flow ** put in list of edges the information of the current edge 
                        if (parentsList[e.to] == -1 && e.capacity > e.flow && e.to != startNode) // check that : 1 - current node is not visited , 2- the capacity allow to pass the flow , 3- the next node is not the start node 
                        {
                            parentsList[e.to] = idx;  // put in parent list the parent of current node 
                            if (e.to == endNode) // the graph end
                                return; // break from the function 
                            q.Enqueue(e.to);  // put in queue the child of of parent 
                        }
                    }
                }
            }

            static int edmondsKarp(int startNode, int endNode)
            {
                int maxFlow = 0;
                while (true) // O(EV)
                {
                    bfs(startNode, endNode); // O(E+V) ( fill parent list )
                    if (parentsList[endNode] == -1) // check that all list is visited from start to end
                    {
                        break;
                    }
                    int flow = int.MaxValue; // initialize the flow with infinity
                    for (int node = parentsList[endNode]; node != -1; node = parentsList[edges[node].from]) // O(E)   (start from end of parent list and break on reaching start and increment by finding parents )
                    {
                        flow = Math.Min(flow, edges[node].capacity - edges[node].flow); //pring min between the infinity and residual ( capacity - flow) ** min in the path
                    }
                    maxFlow += flow;
                    int currentNode = parentsList[endNode]; 
                    while (currentNode != -1) // O(E) loop from end til start and increment with the parent
                    {
                        Arc.add_flow(currentNode, flow, ref edges); // O(E)   (fill the list of edges by calling add_flow )
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
            Console.WriteLine("* * * * * * * * * * * * Dinic * * * * * * * * * * * *");
            TestDinic.Test(test);
            Console.WriteLine("* * * * * * * * * * Edmonds Karp * * * * * * * * * *");
            EdmondsKarp.Test(test);
            Console.WriteLine("* * * * * * * * *  Ford Fulkerson  * * * * * * * * *");
            FordFloukerson.Test(test);
        }
    }
}