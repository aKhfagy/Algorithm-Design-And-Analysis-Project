/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */
#include <iostream>
#include <ostream>
#include <fstream>
#include <string>
#include <map>
#include <vector>
#include <queue>
#include <limits>
#include "testlib.h"

using namespace std;

class Generator {
    class FlowGraph {
    public:
        struct Edge {
            int from, to, capacity, flow;
        };

    private:
        /* List of all - forward and backward - edges */
        vector<Edge> edges;

        /* These adjacency lists store only indices of edges in the edges list */
        vector<vector<size_t> > graph;

    public:
        explicit FlowGraph(size_t n): graph(n) {}

        void add_edge(int from, int to, int capacity) {
            /* Note that we first append a forward edge and then a backward edge,
            * so all forward edges are stored at even indices (starting from 0),
            * whereas backward edges are stored at odd indices in the list edges */
            Edge forward_edge = {from, to, capacity, 0};
            Edge backward_edge = {to, from, 0, 0};
            graph[from].push_back(edges.size());
            edges.push_back(forward_edge);
            graph[to].push_back(edges.size());
            edges.push_back(backward_edge);
        }

        size_t size() const {
            return graph.size();
        }

        const vector<size_t>& get_ids(int from) const {
            return graph[from];
        }

        const Edge& get_edge(size_t id) const {
            return edges[id];
        }

        void add_flow(size_t id, int flow) {
            /* To get a backward edge for a true forward edge (i.e id is even), we should get id + 1
            * due to the described above scheme. On the other hand, when we have to get a "backward"
            * edge for a backward edge (i.e. get a forward edge for backward - id is odd), id - 1
            * should be taken.
            *
            * It turns out that id ^ 1 works for both cases. Think this through! */
            edges[id].flow += flow;
            edges[id ^ 1].flow -= flow;
        }
    };

    vector <int> compute_Gf(const FlowGraph& graph, int s, int t) {
        // This function performs BFS

        vector<int> parent(graph.size(), -1);
        queue<int> frontire;

        frontire.push(s);

        while (!frontire.empty()) {
            int node = frontire.front();
            frontire.pop();

            for (auto id : graph.get_ids(node)) {
                const FlowGraph::Edge& e = graph.get_edge(id);

                if (parent[e.to] == -1 && e.capacity > e.flow && e.to != s) {
                    parent[e.to] = id;
                    frontire.push(e.to);
                }
            }
        }
        
        return parent;
    }

    int max_flow(FlowGraph& graph, int from, int to) {
        int flow = 0;
        
        vector<int> parent;

        do {
            int min = numeric_limits<int>::max();

            parent = compute_Gf(graph, from, to);

            if(parent[to] != -1) {
                
                for (int u = parent[to]; u != -1; u = parent[graph.get_edge(u).from]) 
                    min = std::min(min, 
                    graph.get_edge(u).capacity - graph.get_edge(u).flow);
                
                flow += min;

                for (int u = parent[to]; u != -1; u = parent[graph.get_edge(u).from]) 
                    graph.add_flow(u, min);
            }

        } while(parent[to] != -1);

        return flow;
    }
public:
    void run() {
        int number_of_test_cases = 500;

        for(
            int test_case = 1; 
            test_case <= number_of_test_cases;
            ++test_case) {

            /// test case file
            string path = "tests/" + to_string(test_case) + ".txt";
            ofstream file(path);
            /// Number of nodes and number of edges respictively
            int number_of_nodes = rnd.next(3, 500), number_of_edges = rnd.next(number_of_nodes, 50000);
            
            /// get different sources and sinks
            int source;
            int sink;
            do {
                source = rnd.next(1, number_of_nodes);
                sink = rnd.next(1, number_of_nodes);
            } while(source == sink);

            /// make graph
            FlowGraph graph = FlowGraph(number_of_nodes);
            file << number_of_nodes << ' ' << number_of_edges << '\n';
            file << source << ' ' << sink << '\n';
            for(int i = 0; i < number_of_edges; ++i) {
                int u = rnd.next(1, number_of_nodes);
                int v = rnd.next(1, number_of_nodes);
                int capacity_per_hour = rnd.next(1, int(1e5));
                /// Road between edges u and v that can handle a certain capacity
                file << u << ' ' << v << ' ' << capacity_per_hour << '\n';
                graph.add_edge(u - 1, v - 1, capacity_per_hour);
            }
            file << max_flow(graph, source - 1, sink - 1) << '\n';
            file.close();
        }
    }
};

int main(int argc, char* argv[]) {
    registerGen(argc, argv, 1);
    bool all_files_exist = true;
    std::cout << "Checking if files are present\n";
    for(int file_number = 1; file_number <= 500; ++file_number) {
        std::string path = "tests/" + std::to_string(file_number) + ".txt";
        std::ifstream f(path);
        if(!f.good()) {
            all_files_exist = false;
            break;
        }
        f.close();
    }
    int generator_result;
    if(all_files_exist)
        generator_result = 0;
    else {
        std::cout << "Test cases not present... Please wait for them to be geenrated.\n";
        Generator gen = Generator();
        gen.run();
        std::cout << "Done generating test cases!!.\n\n";
        //return 0;
    }

    std::cout << "Test cases are present...\nProceeeding to test the network flow algorithm...\n";
    int network_flow_result = system("bin/NetworkFlow.exe");
    if(network_flow_result == 0) {
        std::cout << "Congratulations!!! Your program ran successfully without any exceptions.\n";
        std::cout << "To view the running times for analysis please go to running_time folder.\n";
    }
    else 
        std::cerr << "Network Flow program exited unexpectedly\n";

    std::cout << "Press any button to exit...";
    getchar();
    return 0;
} 