﻿using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomPort : Port
{
    public ConnectionType connectionType { get; set; }

    public CustomPort(Orientation orientation, Direction direction, Capacity capacity, ConnectionType connectionType)
        : base(orientation, direction, capacity, typeof(float))
    {
        var listener = new CustomEdgeConnectorListener();
        this.connectionType = connectionType;
        this.m_EdgeConnector = new EdgeConnector<Edge>(listener);
        this.AddManipulator(this.m_EdgeConnector);
        this.portColor = GetPortColor(connectionType);
    }

    private static Color GetPortColor(ConnectionType connectionType)
    {
        return connectionType switch
        {
            ConnectionType.Quest => Color.red,
            ConnectionType.Reward => Color.cyan,
            ConnectionType.Objective => Color.green,
            _ => Color.white
        };
    }

    public class CustomEdgeConnectorListener : IEdgeConnectorListener
    {
        private GraphViewChange m_GraphViewChange;

        private List<Edge> m_EdgesToCreate;

        private List<GraphElement> m_EdgesToDelete;

        public CustomEdgeConnectorListener()
        {
            m_EdgesToCreate = new List<Edge>();
            m_EdgesToDelete = new List<GraphElement>();
            m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            m_EdgesToCreate.Clear();
            m_EdgesToCreate.Add(edge);
            m_EdgesToDelete.Clear();
            if (edge.input.capacity == Capacity.Single)
            {
                foreach (Edge connection in edge.input.connections)
                {
                    if (connection != edge)
                    {
                        m_EdgesToDelete.Add(connection);
                    }
                }
            }

            if (edge.output.capacity == Capacity.Single)
            {
                foreach (Edge connection2 in edge.output.connections)
                {
                    if (connection2 != edge)
                    {
                        m_EdgesToDelete.Add(connection2);
                    }
                }
            }

            if (m_EdgesToDelete.Count > 0)
            {
                graphView.DeleteElements(m_EdgesToDelete);
            }

            List<Edge> edgesToCreate = m_EdgesToCreate;
            if (graphView.graphViewChanged != null)
            {
                edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
            }

            foreach (Edge item in edgesToCreate)
            {
                graphView.AddElement(item);
                edge.input.Connect(item);
                edge.output.Connect(item);
            }
        }
    }
}
