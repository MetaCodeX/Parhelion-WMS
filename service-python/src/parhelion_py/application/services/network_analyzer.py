"""
Network Analyzer Service - Graph Analytics

Analyzes logistics network topology:
- Centrality metrics
- Community detection
- Critical path analysis
- Resilience testing
"""

from typing import Any
from uuid import UUID

import networkx as nx


class NetworkAnalyzer:
    """
    Graph analytics for logistics network.
    
    Uses NetworkX to analyze Hub & Spoke topology,
    identify bottlenecks, and test network resilience.
    """

    def __init__(self):
        self._locations: dict[str, dict] = {}

    async def analyze_network(
        self, tenant_id: UUID
    ) -> dict[str, Any]:
        """
        Complete network analysis.
        
        Args:
            tenant_id: Tenant identifier
            
        Returns:
            Dict with metrics, communities, critical paths, resilience
        """
        # Build graph
        graph = self._build_network_graph()
        
        # Basic metrics
        total_nodes = graph.number_of_nodes()
        total_edges = graph.number_of_edges()
        density = nx.density(graph)
        
        # Centrality analysis
        betweenness = nx.betweenness_centrality(graph)
        closeness = nx.closeness_centrality(graph)
        
        # Critical hubs (top by betweenness)
        critical_hubs = sorted(
            betweenness.items(),
            key=lambda x: x[1],
            reverse=True
        )[:5]
        
        # Community detection (connected components as proxy)
        communities = self._detect_communities(graph)
        
        # Critical paths (high-traffic routes)
        critical_paths = self._find_critical_paths(graph)
        
        # Resilience analysis
        resilience = self._analyze_resilience(graph, critical_hubs)
        
        # Bottleneck detection
        bottlenecks = self._detect_bottlenecks(betweenness)
        
        # Average path length
        try:
            avg_path = nx.average_shortest_path_length(graph)
        except nx.NetworkXError:
            avg_path = 0
        
        return {
            "tenant_id": str(tenant_id),
            "basic_metrics": {
                "total_nodes": total_nodes,
                "total_edges": total_edges,
                "network_density": round(density, 4),
                "average_path_length": round(avg_path, 2),
            },
            "critical_hubs": [
                {
                    "location_id": hub,
                    "code": self._locations.get(hub, {}).get("code", hub),
                    "name": self._locations.get(hub, {}).get("name", "Unknown"),
                    "centrality_score": round(score, 4),
                    "closeness": round(closeness.get(hub, 0), 4),
                }
                for hub, score in critical_hubs
            ],
            "communities": communities,
            "critical_paths": critical_paths,
            "resilience_analysis": resilience,
            "bottlenecks": bottlenecks,
        }

    def _build_network_graph(self) -> nx.Graph:
        """Build undirected graph from network."""
        graph = nx.Graph()
        
        # Sample network
        edges = [
            ("MTY-HUB", "SLP-HUB", {"distance": 450, "time": 4.0}),
            ("MTY-HUB", "TOR-WH", {"distance": 200, "time": 2.5}),
            ("SLP-HUB", "CDMX-HUB", {"distance": 420, "time": 5.0}),
            ("SLP-HUB", "GDL-HUB", {"distance": 310, "time": 3.5}),
            ("GDL-HUB", "CDMX-HUB", {"distance": 540, "time": 5.5}),
            ("CDMX-HUB", "PUE-WH", {"distance": 130, "time": 2.0}),
            ("CDMX-HUB", "VER-HUB", {"distance": 400, "time": 4.5}),
            ("CDMX-HUB", "QRO-WH", {"distance": 220, "time": 2.5}),
        ]
        
        self._locations = {
            "MTY-HUB": {"code": "MTY", "name": "Monterrey Hub", "type": "Hub"},
            "SLP-HUB": {"code": "SLP", "name": "San Luis Potosi Hub", "type": "Hub"},
            "CDMX-HUB": {"code": "CDMX", "name": "Mexico City Hub", "type": "Hub"},
            "GDL-HUB": {"code": "GDL", "name": "Guadalajara Hub", "type": "Hub"},
            "VER-HUB": {"code": "VER", "name": "Veracruz Hub", "type": "Hub"},
            "PUE-WH": {"code": "PUE", "name": "Puebla Warehouse", "type": "Warehouse"},
            "TOR-WH": {"code": "TOR", "name": "Torreon Warehouse", "type": "Warehouse"},
            "QRO-WH": {"code": "QRO", "name": "Queretaro Warehouse", "type": "Warehouse"},
        }
        
        graph.add_edges_from([
            (src, dst, attrs) for src, dst, attrs in edges
        ])
        
        return graph

    def _detect_communities(self, graph: nx.Graph) -> list[dict]:
        """Detect network communities."""
        # Use connected components as simple community detection
        communities = []
        
        # For a connected graph, use modularity-based approach
        try:
            from networkx.algorithms.community import louvain_communities
            comms = louvain_communities(graph, seed=42)
            
            for i, comm in enumerate(comms):
                communities.append({
                    "id": i + 1,
                    "locations": list(comm),
                    "size": len(comm),
                    "density": nx.density(graph.subgraph(comm)),
                })
        except ImportError:
            # Fallback to connected components
            for i, component in enumerate(nx.connected_components(graph)):
                communities.append({
                    "id": i + 1,
                    "locations": list(component),
                    "size": len(component),
                })
        
        return communities

    def _find_critical_paths(self, graph: nx.Graph) -> list[dict]:
        """Find critical paths in the network."""
        critical_paths = []
        
        # Find paths between major hubs
        hubs = [n for n in graph.nodes() if "HUB" in n]
        
        for i, source in enumerate(hubs):
            for target in hubs[i+1:]:
                try:
                    path = nx.shortest_path(graph, source, target)
                    length = nx.shortest_path_length(graph, source, target)
                    
                    critical_paths.append({
                        "origin": source,
                        "destination": target,
                        "path": path,
                        "hops": len(path) - 1,
                        "distance": length,
                    })
                except nx.NetworkXNoPath:
                    pass
        
        return critical_paths[:10]  # Top 10

    def _analyze_resilience(
        self, graph: nx.Graph, critical_hubs: list
    ) -> dict[str, Any]:
        """Analyze network resilience."""
        if not critical_hubs:
            return {"status": "Unable to analyze"}
        
        most_critical = critical_hubs[0][0]
        
        # Create graph without most critical hub
        test_graph = graph.copy()
        test_graph.remove_node(most_critical)
        
        # Check connectivity
        is_connected = nx.is_connected(test_graph)
        num_components = nx.number_connected_components(test_graph)
        
        # Find isolated nodes
        isolated = []
        if not is_connected:
            for component in nx.connected_components(test_graph):
                if len(component) < 3:
                    isolated.extend(list(component))
        
        return {
            "most_critical_hub": {
                "id": most_critical,
                "name": self._locations.get(most_critical, {}).get("name", "Unknown"),
            },
            "is_connected_after_removal": is_connected,
            "components_after_removal": num_components,
            "isolated_locations": isolated,
            "resilience_score": 1.0 if is_connected else 1.0 / num_components,
        }

    def _detect_bottlenecks(
        self, betweenness: dict[str, float]
    ) -> list[dict]:
        """Detect network bottlenecks."""
        bottlenecks = []
        
        threshold = 0.15  # Nodes with >15% of paths passing through
        
        for node, score in betweenness.items():
            if score > threshold:
                bottlenecks.append({
                    "location_id": node,
                    "code": self._locations.get(node, {}).get("code", node),
                    "name": self._locations.get(node, {}).get("name", "Unknown"),
                    "centrality": round(score, 4),
                    "severity": "HIGH" if score > 0.3 else "MEDIUM",
                })
        
        return sorted(bottlenecks, key=lambda x: x["centrality"], reverse=True)
