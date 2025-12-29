"""
Route Optimizer Service - NetworkX Graph Algorithms

Optimizes logistics routes using graph algorithms:
- Dijkstra: Shortest path by time
- A*: With geographic heuristic
- Yen's K-shortest: Top K alternative routes
"""

from typing import Any
from uuid import UUID

import networkx as nx

from parhelion_py.infrastructure.external.parhelion_client import ParhelionApiClient


class RouteOptimizer:
    """
    Route optimization using NetworkX graph algorithms.
    
    Builds a graph from NetworkLinks and calculates optimal paths
    considering transit time, distance, and capacity constraints.
    """

    def __init__(self):
        self._graph: nx.DiGraph | None = None
        self._locations: dict[str, dict] = {}

    async def calculate_optimal_route(
        self,
        tenant_id: UUID,
        origin_id: str,
        destination_id: str,
        constraints: dict[str, Any] | None = None,
    ) -> dict[str, Any]:
        """
        Calculate the optimal route between two locations.
        
        Args:
            tenant_id: Tenant for multi-tenant filtering
            origin_id: Origin location ID
            destination_id: Destination location ID
            constraints: Optional constraints (max_time, avoid_locations, etc.)
            
        Returns:
            Dict with optimal_path, total_time, alternatives, bottlenecks
        """
        constraints = constraints or {}
        
        # Build graph from network data
        graph = await self._build_network_graph(tenant_id)
        
        # Apply constraints (remove avoided locations, etc.)
        graph = self._apply_constraints(graph, constraints)
        
        # Validate nodes exist
        if origin_id not in graph:
            return {"error": f"Origin {origin_id} not found in network"}
        if destination_id not in graph:
            return {"error": f"Destination {destination_id} not found in network"}
        
        # Check if path exists
        if not nx.has_path(graph, origin_id, destination_id):
            return {"error": "No path exists between origin and destination"}
        
        # Calculate optimal path (Dijkstra)
        try:
            optimal_path = nx.dijkstra_path(
                graph, origin_id, destination_id, weight="transit_time"
            )
            optimal_cost = nx.dijkstra_path_length(
                graph, origin_id, destination_id, weight="transit_time"
            )
        except nx.NetworkXNoPath:
            return {"error": "No path exists between origin and destination"}
        
        # Calculate alternative paths (K-shortest)
        alternatives = self._find_k_shortest_paths(
            graph, origin_id, destination_id, k=3
        )
        
        # Detect bottlenecks (high betweenness on path)
        bottlenecks = self._detect_bottlenecks(graph, optimal_path)
        
        # Calculate total distance
        total_distance = self._calculate_total_distance(graph, optimal_path)
        
        # Get path details
        path_details = [
            {
                "id": node,
                "code": self._locations.get(node, {}).get("code", node),
                "name": self._locations.get(node, {}).get("name", "Unknown"),
                "type": self._locations.get(node, {}).get("type", "Unknown"),
            }
            for node in optimal_path
        ]
        
        return {
            "optimal_path": path_details,
            "total_time_hours": round(optimal_cost, 2),
            "total_distance_km": round(total_distance, 2),
            "hops": len(optimal_path) - 1,
            "algorithm": "dijkstra",
            "confidence_score": self._calculate_confidence(graph, optimal_path),
            "alternatives": alternatives,
            "bottlenecks": bottlenecks,
        }

    async def _build_network_graph(self, tenant_id: UUID) -> nx.DiGraph:
        """Build NetworkX graph from API data."""
        graph = nx.DiGraph()
        
        # In a real implementation, fetch from API
        # For now, create a sample network
        async with ParhelionApiClient() as client:
            if await client.health_check():
                # TODO: Implement actual network fetch
                pass
        
        # Sample network structure (Hub & Spoke)
        sample_links = [
            ("MTY-HUB", "SLP-HUB", {"transit_time": 4.0, "distance_km": 450}),
            ("MTY-HUB", "CDMX-HUB", {"transit_time": 9.0, "distance_km": 920}),
            ("SLP-HUB", "CDMX-HUB", {"transit_time": 5.0, "distance_km": 420}),
            ("SLP-HUB", "GDL-HUB", {"transit_time": 3.5, "distance_km": 310}),
            ("GDL-HUB", "CDMX-HUB", {"transit_time": 5.5, "distance_km": 540}),
            ("CDMX-HUB", "PUE-WH", {"transit_time": 2.0, "distance_km": 130}),
            ("CDMX-HUB", "VER-HUB", {"transit_time": 4.5, "distance_km": 400}),
            ("MTY-HUB", "TOR-WH", {"transit_time": 2.5, "distance_km": 200}),
        ]
        
        self._locations = {
            "MTY-HUB": {"code": "MTY", "name": "Monterrey Hub", "type": "Hub"},
            "SLP-HUB": {"code": "SLP", "name": "San Luis Potosi Hub", "type": "Hub"},
            "CDMX-HUB": {"code": "CDMX", "name": "Ciudad de Mexico Hub", "type": "Hub"},
            "GDL-HUB": {"code": "GDL", "name": "Guadalajara Hub", "type": "Hub"},
            "VER-HUB": {"code": "VER", "name": "Veracruz Hub", "type": "Hub"},
            "PUE-WH": {"code": "PUE", "name": "Puebla Warehouse", "type": "Warehouse"},
            "TOR-WH": {"code": "TOR", "name": "Torreon Warehouse", "type": "Warehouse"},
        }
        
        for source, target, attrs in sample_links:
            graph.add_edge(source, target, **attrs)
            # Add reverse edge (bidirectional)
            graph.add_edge(target, source, **attrs)
        
        return graph

    def _apply_constraints(
        self, graph: nx.DiGraph, constraints: dict[str, Any]
    ) -> nx.DiGraph:
        """Apply routing constraints to the graph."""
        g = graph.copy()
        
        # Remove avoided locations
        avoid_locations = constraints.get("avoid_locations", [])
        for loc in avoid_locations:
            if loc in g:
                g.remove_node(loc)
        
        # Filter by max transit time per edge
        max_time = constraints.get("max_edge_time")
        if max_time:
            edges_to_remove = [
                (u, v) for u, v, d in g.edges(data=True)
                if d.get("transit_time", 0) > max_time
            ]
            g.remove_edges_from(edges_to_remove)
        
        return g

    def _find_k_shortest_paths(
        self, graph: nx.DiGraph, source: str, target: str, k: int = 3
    ) -> list[dict[str, Any]]:
        """Find K shortest paths using Yen's algorithm."""
        alternatives = []
        
        try:
            paths = list(nx.shortest_simple_paths(
                graph, source, target, weight="transit_time"
            ))
            
            # Skip first (it's the optimal), take next k-1
            for i, path in enumerate(paths[1:k], start=2):
                cost = sum(
                    graph[path[j]][path[j+1]].get("transit_time", 0)
                    for j in range(len(path) - 1)
                )
                alternatives.append({
                    "rank": i,
                    "path": path,
                    "time_hours": round(cost, 2),
                    "hops": len(path) - 1,
                })
        except nx.NetworkXNoPath:
            pass
        
        return alternatives

    def _detect_bottlenecks(
        self, graph: nx.DiGraph, path: list[str]
    ) -> list[dict[str, Any]]:
        """Detect bottleneck nodes on the path."""
        if len(path) < 3:
            return []
        
        # Calculate betweenness centrality
        betweenness = nx.betweenness_centrality(graph)
        
        bottlenecks = []
        for node in path[1:-1]:  # Exclude origin and destination
            score = betweenness.get(node, 0)
            if score > 0.3:  # Threshold for bottleneck
                bottlenecks.append({
                    "location_id": node,
                    "code": self._locations.get(node, {}).get("code", node),
                    "name": self._locations.get(node, {}).get("name", "Unknown"),
                    "centrality_score": round(score, 3),
                    "severity": "HIGH" if score > 0.5 else "MEDIUM",
                })
        
        return bottlenecks

    def _calculate_total_distance(
        self, graph: nx.DiGraph, path: list[str]
    ) -> float:
        """Calculate total distance of a path in km."""
        total = 0.0
        for i in range(len(path) - 1):
            edge_data = graph.get_edge_data(path[i], path[i+1], {})
            total += edge_data.get("distance_km", 0)
        return total

    def _calculate_confidence(
        self, graph: nx.DiGraph, path: list[str]
    ) -> float:
        """Calculate confidence score for the route (0-1)."""
        # Simple heuristic based on path length and edge reliability
        if len(path) < 2:
            return 0.0
        
        # Fewer hops = higher confidence
        hop_score = max(0, 1 - (len(path) - 2) * 0.1)
        
        # All edges have data = higher confidence
        edge_score = 1.0
        for i in range(len(path) - 1):
            if not graph.has_edge(path[i], path[i+1]):
                edge_score -= 0.2
        
        return round(min(1.0, hop_score * 0.5 + edge_score * 0.5), 2)
