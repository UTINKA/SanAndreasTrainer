using ProcessMemoryReaderLib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace projSanAndreasTrainer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Process[] SA;
        ProcessModule mainModule;
        ProcessMemoryReader mem = new ProcessMemoryReader();

        struct offsets
        {
            static public int health, baseAddr;
            static public int[] xPos, zPos, yPos;
            // the game which uses XZY and not XYZ
        }

        bool gameFound = false;

        float[,] teleCoords = new float[2 , 3];

        private void btnAttach_Click(object sender , EventArgs e)
        {
            try
            {
                SA = Process.GetProcessesByName("gta_sa");
                mainModule = SA[0].MainModule;
                mem.ReadProcess = SA[0];
                mem.OpenProcess();
                gameFound = true;

                offsets.baseAddr = 0xB6F5F0;
                offsets.health = 0x540;
                offsets.xPos = new int[] { 0x14 , 0x30 };
                offsets.zPos = new int[] { 0x14 , 0x34 };
                offsets.yPos = new int[] { 0x14 , 0x38 };

                btnAttach.BackColor = Color.Green; //User Feedback
                btnAttach.Enabled = false;
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show("Game not found!");
                //throw ex;
            }
        }

        private void tmrProcess_Tick(object sender , EventArgs e)
        {
            if (gameFound && !SA[0].HasExited)
            {
                // Only the base address or multiLevel addresses need to be established first
                int playerBaseAddress = mem.ReadInt(offsets.baseAddr);
                // Stores an address which is the base address of the player struct.
                int XAddress = mem.ReadMultiLevelPointer(offsets.baseAddr , 4 , offsets.xPos);
                // Multi level pointer with 2 offsets is needed to find the address of X position,
                // then this address can be read as a float.
                int ZAddress = mem.ReadMultiLevelPointer(offsets.baseAddr , 4 , offsets.zPos);
                int YAddress = mem.ReadMultiLevelPointer(offsets.baseAddr , 4 , offsets.yPos);


                lblX.Text = mem.ReadFloat(XAddress).ToString();
                lblZ.Text = mem.ReadFloat(ZAddress).ToString();
                lblY.Text = mem.ReadFloat(YAddress).ToString();

                lblHealth.Text = mem.ReadFloat(playerBaseAddress + offsets.health).ToString();

                int Hotkey = ProcessMemoryReaderApi.GetKeyState(0x74);//F5
                if ((Hotkey & 0x8000) != 0)
                {
                    // Teleport to Grove Street
                    mem.WriteFloat(XAddress , 2495);
                    mem.WriteFloat(ZAddress , -1668);
                    mem.WriteFloat(YAddress , 13);
                }

                int Hotkey2 = ProcessMemoryReaderApi.GetKeyState(0x75);//F6
                if ((Hotkey2 & 0x8000) != 0)
                {
                    // Teleport to Dome Stadium Roof
                    mem.WriteFloat(XAddress , 2737);
                    mem.WriteFloat(ZAddress , -1760);
                    mem.WriteFloat(YAddress , 44);
                }

                int Hotkey3 = ProcessMemoryReaderApi.GetKeyState(0x76);//F7
                if ((Hotkey3 & 0x8000) != 0)
                {
                    // Teleport to Skyscraper
                    mem.WriteFloat(XAddress , 1544);
                    mem.WriteFloat(ZAddress , -1353);
                    mem.WriteFloat(YAddress , 330);
                }
            }
            else
            {
                // Game has ended so stop performing readMemory etc
                gameFound = false;
                btnAttach.BackColor = Color.Red;
                btnAttach.Enabled = true;
            }
        }//tmrProcess
    }
}
