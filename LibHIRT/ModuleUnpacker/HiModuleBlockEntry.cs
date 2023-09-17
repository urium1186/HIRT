namespace LibHIRT.ModuleUnpacker
{
    //  # Table 4
    internal class HiModuleBlockEntry
    {
        int comp_offset; // = gf.read_uint32(fb)  # 0x00
        int comp_size; // = gf.read_uint32(fb)  # 0x04
        int decomp_offset; // = gf.read_uint32(fb)  # 0x08
        int decomp_size; // = gf.read_uint32(fb)  # 0x0C
        bool b_compressed; // = gf.read_uint32(fb)  # 0x10

        public int Comp_offset { get => comp_offset; set => comp_offset = value; }
        public int Comp_size { get => comp_size; set => comp_size = value; }
        public int Decomp_offset { get => decomp_offset; set => decomp_offset = value; }
        public int Decomp_size { get => decomp_size; set => decomp_size = value; }
        public bool B_compressed { get => b_compressed; set => b_compressed = value; }

        public void ReadIn(BinaryReader binaryReader)
        {
            comp_offset = binaryReader.ReadInt32();
            comp_size = binaryReader.ReadInt32();
            decomp_offset = binaryReader.ReadInt32();
            decomp_size = binaryReader.ReadInt32();
            b_compressed = binaryReader.ReadUInt32() == 1;
        }
    }


}
