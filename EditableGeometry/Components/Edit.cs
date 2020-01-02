using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EditableGeometry.Core;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace EditableGeometry.Components
{
    public class Edit : GH_Component
    {
        private Geometry Geometry;
        private bool _edit_mode = false;

        public bool EditMode
        {
            get => _edit_mode;
            set
            {
                _edit_mode = value;
                if (_edit_mode) Message = "Edit mode";
                else Message = "";
            }
        }

        /// <summary>
        /// Initializes a new instance of the Edit class.
        /// </summary>
        public Edit()
          : base("Edit", "Edit",
              "Edit geometry in rhino viewport",
              EditableGeometryInfo.MAIN_CATEGORY, EditableGeometryInfo.SUBCATEGORY_EDIT)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to edit", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Flush", "F", "Removes all edits", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Edited Geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase geo = null;
            bool flush = false;

            if (!DA.GetData(0, ref geo)) return;
            if(Geometry == null) Geometry = new Geometry(geo);

            if (!DA.GetData(1, ref flush)) return;
            if (flush)
            {
                // Flush edit cache
                Geometry = null;
                return;
            }

            if (EditMode)
            {
                Geometry.MakeEditable(RhinoDoc.ActiveDoc);
            }

            else
            {
                Geometry.EndEdit(RhinoDoc.ActiveDoc);
            }


            DA.SetData(0, Geometry.GetGeometry());

        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem item = Menu_AppendItem(menu, "Edit mode", Menu_EditMode_Clicked, true, EditMode);
            item.ToolTipText = "When checked, component will be in active edit mode";
        }

        private void Menu_EditMode_Clicked(object sender, EventArgs e)
        {
            RecordUndoEvent("EditMode");
            EditMode = !EditMode;
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("EditMode", EditMode);
            var bytes = GH_Convert.CommonObjectToByteArray(Geometry.GetGeometry());

            // When closing rhino this is null :/
            if(bytes != null) writer.SetByteArray("Geometry", bytes);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            EditMode = reader.GetBoolean("EditMode");
            Geometry = new Geometry(GH_Convert.ByteArrayToCommonObject<GeometryBase>(reader.GetByteArray("Geometry")));
            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("42d6a117-0f7d-4930-a4a6-4a56a13982f4"); }
        }
    }
}