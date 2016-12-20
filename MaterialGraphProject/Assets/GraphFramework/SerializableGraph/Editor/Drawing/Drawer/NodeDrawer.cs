using System.Collections.Generic;
using System.Linq;
using RMGUI.GraphView;
using UnityEngine;
using UnityEngine.RMGUI;
using UnityEditor.Graphing.Util;
using UnityEngine.RMGUI.StyleSheets;

namespace UnityEditor.Graphing.Drawing
{
    public class NodeDrawer : GraphElement
    {
        protected VisualContainer m_LeftContainer;
        protected VisualContainer m_RightContainer;
        HeaderDrawer m_HeaderDrawer;
        VisualContainer m_InputContainer;
        VisualContainer m_OutputContainer;
        List<AnchorDrawData> m_CurrentAnchors;
        VisualContainer m_ControlsContainer;
        List<ControlDrawData> m_CurrentControlDrawData;
        bool m_CurrentExpanded;

        public NodeDrawer()
        {
            content = new GUIContent("");
            AddContainers();
            classList = new ClassList("Node");
        }

        private void AddContainers()
        {
            /* 
             * Layout structure:
             * node
             * - left
             * - - header
             * - - input
             * - - controls
             * - right
             * - - output
             */

            m_LeftContainer = new VisualContainer
            {
                classList = new ClassList("pane", "left"),
                pickingMode = PickingMode.Ignore
            };
            AddChild(m_LeftContainer);

            m_RightContainer = new VisualContainer
            {
                classList = new ClassList("pane", "right"),
                pickingMode = PickingMode.Ignore
            };
            AddChild(m_RightContainer);

            m_HeaderDrawer = new HeaderDrawer();
            m_HeaderDrawer.AddToClassList("paneItem");
            m_LeftContainer.AddChild(m_HeaderDrawer);

            m_InputContainer = new VisualContainer
            {
                name = "input",
                pickingMode = PickingMode.Ignore,
            };
            m_InputContainer.AddToClassList("paneItem");
            m_LeftContainer.AddChild(m_InputContainer);

            m_ControlsContainer = new VisualContainer
            {
                name = "controls",
                pickingMode = PickingMode.Ignore,
            };
            m_ControlsContainer.AddToClassList("paneItem");
            m_LeftContainer.AddChild(m_ControlsContainer);

            m_OutputContainer = new VisualContainer
            {
                name = "output",
                pickingMode = PickingMode.Ignore,
            };
            m_OutputContainer.AddToClassList("paneItem");
            m_RightContainer.AddChild(m_OutputContainer);

            m_CurrentAnchors = new List<AnchorDrawData>();

            m_CurrentControlDrawData = new List<ControlDrawData>();
        }

        private void AddHeader(NodeDrawData nodeData)
        {
            var headerData = nodeData.elements.OfType<HeaderDrawData>().FirstOrDefault();
            m_HeaderDrawer.dataProvider = headerData;
        }

        private void AddSlots(NodeDrawData nodeData)
        {
            var anchors = nodeData.elements.OfType<AnchorDrawData>().ToList();

            if (anchors.Count == 0)
            {
                m_RightContainer.AddToClassList("empty");
                return;
            }

            if (anchors.ItemsReferenceEquals(m_CurrentAnchors) && m_CurrentExpanded == nodeData.expanded)
                return;

            m_CurrentAnchors = anchors;
            m_InputContainer.ClearChildren();
            m_OutputContainer.ClearChildren();

            int outputCount = 0;

            foreach (var anchor in anchors)
            {
                var hidden = !nodeData.expanded && !anchor.connected;
                if (!hidden && anchor.direction == Direction.Input)
                {
                    m_InputContainer.AddChild(new NodeAnchor(anchor));
                }
                else if (!hidden && anchor.direction == Direction.Output)
                {
                    outputCount++;
                    m_OutputContainer.AddChild(new NodeAnchor(anchor));
                }
            }

            if (outputCount == 0)
                m_RightContainer.AddToClassList("empty");
            else
                m_RightContainer.RemoveFromClassList("empty");
        }

        private void AddControls(NodeDrawData nodeData)
        {
            var controlDrawData = nodeData.elements.OfType<ControlDrawData>().ToList();

            if (!nodeData.expanded)
            {
                m_ControlsContainer.ClearChildren();
                m_CurrentControlDrawData.Clear();
                return;
            }

            if (controlDrawData.ItemsReferenceEquals(m_CurrentControlDrawData))
            {
                for (int i = 0; i < controlDrawData.Count; i++)
                {
                    var controlData = controlDrawData[i];
                    var imContainer = m_ControlsContainer.GetChildAtIndex(i) as IMGUIContainer;
                    imContainer.OnGUIHandler = controlData.OnGUIHandler;
                    imContainer.height = controlData.GetHeight();
                }
            }
            else
            {
                m_ControlsContainer.ClearChildren();
                m_CurrentControlDrawData.Clear();

                foreach (var controlData in controlDrawData)
                {
                    var imContainer = new IMGUIContainer()
                    {
                        name = "element",
                        OnGUIHandler = controlData.OnGUIHandler,
                        pickingMode = PickingMode.Position,
                        height = controlData.GetHeight()
                    };
                    m_ControlsContainer.AddChild(imContainer);
                    m_CurrentControlDrawData.Add(controlData);
                }
            }
        }

        public override void OnDataChanged()
        {
            base.OnDataChanged();

            var nodeData = dataProvider as NodeDrawData;

            if (nodeData == null)
            {
                ClearChildren();
                AddContainers();
                return;
            }

            if (!nodeData.expanded)
                AddToClassList("collapsed");
            else
                RemoveFromClassList("collapsed");

            AddHeader(nodeData);
            AddSlots(nodeData);
            AddControls(nodeData);

            m_CurrentExpanded = nodeData.expanded;
        }
    }
}