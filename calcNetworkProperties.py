import sys
import numpy as np
import networkx as nx
from perseuspy import pd
from perseuspy import read_networks, write_networks
from perseuspy.parameters import *

pd.options.mode.chained_assignment = None  # turns off chained assignment warning

_, paramfile, infolder, outfolder = sys.argv

import time
start_time = time.time()

##### parse Perseus parameters
p = parse_parameters(paramfile)
given_prop = multiChoiceParam(p, 'Properties').values()
directed = singleChoiceParam(p, 'Graph type')
directed = 1 if directed == 'Directed' else 0
which_prop = []
allProperties = ["Network \t Density", "Network \t Average node connectivity", "Network \t Node connectivity", "Network \t Edge connectivity", "Network \t Diameter", "Network \t Radius", "Node \t Degree", "Node \t Out-degree", "Node \t Degree centrality", "Node \t In-degree centrality", "Node \t Out-degree centrality", "Node \t Closeness centrality", "Node \t Betweenness centrality", "Node \t Average neighbor degree", "Node \t Clustering coefficient", "Edge \t Betweenness centrality"]    
for property in given_prop:
    which_prop.append(allProperties.index(property))
#####

def add_undirected_edge_properties(table, dict, columnname):
    """
    This function adds undirected edge properties on directed edge table
    :param table: edgetable
    :param dict: dictionary with keys edge-tuples and values property values
    :param columnname: string for new columnname
    :returns edgetable with one more property column
    """
    table[columnname] = np.nan
    for key in dict:
        i = table[(table['Source'] == key[0]) & (table['Target'] == key[1])].index.tolist().pop(0)
        j = table[(table['Source'] == key[1]) & (table['Target'] == key[0])].index.tolist().pop(0)
        table[columnname][i] = dict[key]
        table[columnname][j] = dict[key]
    return table

def calc_network_properties(allDicts):
    """
    This function calculates network properties for all existing networks
    :param allDicts: array of networktable and 4 dictionaries (nodes, edges, nameGUID, graphs)
    :returns array of same 5 objects with added property columns
    """
    networks = allDicts[0]
    nodes = allDicts[1]
    edges = allDicts[2]
    nameGUID = allDicts[3]
    graphs = allDicts[4]
    if 0 in which_prop:
        networks['Density'] = np.nan
    if 1 in which_prop:
        networks['Avg_node_connectivity'] = np.nan
    if 2 in which_prop:
        networks['Node_connectivity'] = np.nan
    if 3 in which_prop:
        networks['Edge_connectivity'] = np.nan
    if 4 in which_prop:
        networks['Diameter'] = np.nan
    if 5 in which_prop:
        networks['Radius'] = np.nan
    
    for Name, GUID in zip(networks.Name, networks.GUID):
        G = graphs[GUID]
        # Abort script if duplicate edges occur
        if ((directed == 1 and len(G.edges()) != len(edges[GUID].index)) or (directed == 0 and len(G.edges()) * 2 != len(edges[GUID].index))):
            sys.exit("InputError: One or more networks are containing duplicate edges/nodes. Please remove these before executing this script. (>>Collapse duplicate rows<<)")            
    # Network level
        netwIndex = networks[networks['GUID'] == GUID].index.tolist()
        i = netwIndex.pop(0)
        if 0 in which_prop:
            networks['Density'][i] = nx.density(G) # number of edges / number of possible edges
        if 1 in which_prop:
            networks['Avg_node_connectivity'] = nx.average_node_connectivity(G)
        if 2 in which_prop:
            networks['Node_connectivity'] = nx.node_connectivity(G) # robustness indicator
        if 3 in which_prop:
            networks['Edge_connectivity'] = nx.edge_connectivity(G) # robustness indicator
        if 4 in which_prop:
            networks['Diameter'] = nx.diameter(G)#if not nx.is_directed(G) and nx.is_connected(G):
        if 5 in which_prop:
            networks['Radius'] = nx.radius(G)
            
    # Node level
        if 6 in which_prop:
            nodes[GUID]['Degree'] = pd.DataFrame({'Degree':list(nx.degree(G).values())}) # degree
        if 7 in which_prop:
            nodes[GUID]['Out_Degree'] = pd.DataFrame({'Out_Degree':list(G.out_degree().values())}) # degree
        if 8 in which_prop:
            nodes[GUID]['Degree_centrality'] = pd.DataFrame({'Degree_centrality':list(nx.degree_centrality(G).values())}) # degree/ max. possible degree    
        if 9 in which_prop:
            nodes[GUID]['In_Degree_centrality'] = pd.DataFrame({'In_Degree_centrality':list(nx.in_degree_centrality(G).values())}) # degree/ max. possible degree
        if 10 in which_prop:
            nodes[GUID]['Out_Degree_centrality'] = pd.DataFrame({'Out_Degree_centrality':list(nx.out_degree_centrality(G).values())}) # degree/ max. possible degree
        if 11 in which_prop:
            nodes[GUID]['Closeness_centrality'] = pd.DataFrame({'Closeness_centrality':list(nx.closeness_centrality(G).values())}) # sum of the length of the shortest paths between the node and all other nodes[GUID]
        if 12 in which_prop:
            nodes[GUID]['Betweenness_centrality'] = pd.DataFrame({'Betweenness_centrality':list(nx.betweenness_centrality(G).values())}) # sum over all pairs of nodes[GUID]: number of shortest paths through v / number of all shortest paths
        if 13 in which_prop:
            nodes[GUID]['Avg_neighbor_deg'] = pd.DataFrame({'Avg_neighbor_deg':list(nx.average_neighbor_degree(G).values())})
        if 14 in which_prop:
            nodes[GUID]['Clustering_coefficient'] = pd.DataFrame({'Clustering_coefficient':list(nx.clustering(G).values())})#if not nx.is_directed(G):

    # Edge level
        if 15 in which_prop:
            if directed == 0:
                edges[GUID] = add_undirected_edge_properties(edges[GUID], nx.edge_betweenness_centrality(G), 'Edge_Betweenness_Centrality')
            else:
                edges[GUID]['Edge_betweenness_centrality'] = pd.DataFrame({'Edge_betweenness_centrality':list(nx.edge_betweenness_centrality(G).values())}) # number of shortest paths that pass through the edge

    allDicts[0] = networks
    allDicts[1] = nodes
    allDicts[2] = edges
    return allDicts

# Workflow
allDicts = read_networks(infolder, directed)
newAllDicts = calc_network_properties(allDicts)
write_networks(outfolder, newAllDicts)

print("--- %s seconds ---" % (time.time() - start_time))






