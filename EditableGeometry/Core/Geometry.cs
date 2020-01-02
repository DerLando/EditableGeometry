﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Geometry.Voronoi;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace EditableGeometry.Core
{
    public class Geometry
    {
        public Guid Guid { get; set; } = Guid.Empty;
        public GeometryBase GeometryBase { get; set; }
        public ObjectType ObjectType { get; set; }
        public HistoryRecord HistoryRecord { get; set; }

        public Geometry(GeometryBase geometryBase)
        {
            GeometryBase = geometryBase;
        }

        public void MakeEditable(RhinoDoc doc)
        {
            if (Guid == Guid.Empty)
            {
                Guid = doc.Objects.Add(GeometryBase);
            }

            else
            {
                doc.Objects.Delete(Guid, true);
                doc.Objects.Add(GeometryBase, new ObjectAttributes {ObjectId = Guid});
            }
        }

        public void EndEdit(RhinoDoc doc)
        {
            var geo = doc.Objects.FindGeometry(Guid);
            if (geo is null) return;
            GeometryBase = geo;
            doc.Objects.Delete(Guid, true);
            Guid = Guid.Empty;
        }

        public GeometryBase GetGeometry()
        {
            return GeometryBase;
        }
    }
}
