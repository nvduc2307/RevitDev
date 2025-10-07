﻿using Autodesk.Revit.DB.DirectContext3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.Virture
{
    public sealed class RenderingBufferStorage
    {
        public VertexFormatBits FormatBits { get; set; }
        public int PrimitiveCount { get; set; }
        public int VertexBufferCount { get; set; }
        public int IndexBufferCount { get; set; }
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public VertexFormat VertexFormat { get; set; }
        public EffectInstance EffectInstance { get; set; }

        public bool IsValid()
        {
            if (!VertexBuffer.IsValid()) return false;
            if (!IndexBuffer.IsValid()) return false;
            if (!VertexFormat.IsValid()) return false;
            if (!EffectInstance.IsValid()) return false;

            return true;
        }
    }
}
