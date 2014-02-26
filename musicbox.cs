//!csc -nologo -debug+ -t:winexe "%file%" -r:midi.dll -res:open.bmp -res:save.bmp -res:new.bmp -res:musicbox.ico -win32icon:musicbox.ico
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using Midi;

class PrintDoc : PrintDocument {
	public PrintDoc(BitArray[] composizione,string title) {
		this.composizione=composizione;
		this.title=title;
	}
	BitArray[] composizione;
	string title;
	protected override void OnBeginPrint(System.Drawing.Printing.PrintEventArgs e) {
		base.OnBeginPrint(e);
	}
	protected override void OnPrintPage(System.Drawing.Printing.PrintPageEventArgs e) {
		base.OnPrintPage(e);
		
		int i,j;
		float dx=(70-6.5F*2)/19;
		StringFormat sf=new StringFormat();
		sf.Alignment=StringAlignment.Center;
		sf.LineAlignment=StringAlignment.Center;
		Graphics g=e.Graphics;
		g.Transform=new Matrix(new Rectangle(0,0,1,1),
			new PointF[]{
				new PointF(10,0),
				new PointF(11,0),
				new PointF(10,1)
			});
		g.PageUnit=GraphicsUnit.Millimeter;
		using (Pen pen=new Pen(Color.Black,0.1F)) {
			g.DrawRectangle(pen,0,15,70,180);
			RectangleF r=new RectangleF(6.5F,20F,70-6.5F*2,22);
			g.DrawRectangle(pen,Rectangle.Ceiling(r));
			
			using (Font font=new Font("Arial",24))
				g.DrawString(title,font,Brushes.Black,r,sf);
			using (Font font=new Font("Segoe UI Symbol",128))
				g.DrawString("\u266B",font,Brushes.LightGray,70/2,130,sf);
			
			using (Font font=new Font("Arial",10)) {
				for (i=0;i<20;i++) {
					g.DrawString(((char)(65+((i+2)%7))).ToString(),font,Brushes.Black,6.5F+dx*i,48,sf);
					g.DrawLine(pen,6.5F+dx*i,50,6.5F+dx*i,190);
				}
			}
			for (i=0;i<35;i++)
				g.DrawLine(pen,6.5F,50+i*4,70-6.5F,50+i*4);
			for (i=0;i<composizione.Length;i++) {
				j=0;
				if (composizione[i]!=null)
					foreach (bool b in composizione[i]) {
						if (b)
							g.FillEllipse(Brushes.Black,6.5F+j*dx-1.5F,50+(i+1)*4-1.5F,3,3);
						j++;
					}
				}
		}
		e.HasMorePages=false;
	}
}

class App : Form {
	const int MAXNOTE=400;
	const int DX=20;
	Panel pnl=new Panel();
	PictureBox pbKeyboard=new PictureBox(),
		pbCanvas=new PictureBox();
	ToolStripMenuItem tsmiFile=new ToolStripMenuItem("&File"),
		tsmiPlay;

	int lastX=-1, lastY=-1, currentNote, lastNote;
	string sFileName=null;
	bool playing=false, bDirty=false;
	BitArray[] composizione=new BitArray[MAXNOTE];
	Timer t=new Timer();
		OutputDevice outputDevice=OutputDevice.InstalledDevices[1];

	void pbCanvas_MouseMove(object sender,MouseEventArgs e) {
		int w=pbCanvas.ClientSize.Width,
			h=pbCanvas.ClientSize.Height;
		int d=h/20;
		using (Graphics g=pbCanvas.CreateGraphics()) {
			int X=e.X-e.X%DX,
				Y=e.Y-e.Y%d;
			
			if ((X!=lastX || Y!=lastY) && Y/d<20) {
				if (lastX>=0)
					ControlPaint.FillReversibleRectangle(new Rectangle(pbCanvas.PointToScreen(new Point(lastX,lastY)),new Size(DX,DX)),Color.Gray);
				ControlPaint.FillReversibleRectangle(new Rectangle(pbCanvas.PointToScreen(new Point(X,Y)),new Size(DX,DX)),Color.Gray);
				lastX=X;
				lastY=Y;
			}
		}
	}
	
	void pbCanvas_MouseUp(object sender,MouseEventArgs e) {
		int w=pbCanvas.ClientSize.Width,
			h=pbCanvas.ClientSize.Height,
			i, d=h/20, nota=19-e.Y/d, t=e.X/DX;
		if (System.Windows.Forms.Control.ModifierKeys==Keys.Control) {	//delete
			for (i=t;i<MAXNOTE-1;i++)
				composizione[i]=composizione[i+1];
			composizione[MAXNOTE-1]=null;
			SetDirty(true);
		}
		else if (System.Windows.Forms.Control.ModifierKeys==Keys.Shift) {	//insert
			for (i=MAXNOTE-1;i>=t;i--)
				composizione[i]=composizione[i-1];
			composizione[t]=null;
			SetDirty(true);
		}
		else if (System.Windows.Forms.Control.ModifierKeys==Keys.Alt) {	//play
			currentNote=t;
			OnPlay(this,EventArgs.Empty);
		}
		else {		//set
			if (composizione[t]==null) composizione[t]=new BitArray(20);
			composizione[t][nota]=!composizione[t][nota];
			SetDirty(true);
		}
		pbCanvas.Refresh();
		lastX=lastY=-1;
	}

	void pbCanvas_MouseLeave(object sender,EventArgs e) {
		int h=pbCanvas.ClientSize.Height;
		if (lastX>=0) {
			ControlPaint.FillReversibleRectangle(new Rectangle(pbCanvas.PointToScreen(new Point(lastX,lastY)),new Size(DX,DX)),Color.Gray);
			lastX=lastY=-1;
		}
	}	

	void pbCanvas_OnPaint(object sender,PaintEventArgs e) {
		int i, w=pbCanvas.ClientSize.Width,
			h=pbCanvas.ClientSize.Height,
			d=h/20;
		
		using (Pen pen=new Pen(Color.Gray,2)) {
			for (i=0;i<20;i++)
				e.Graphics.DrawLine((i%7==5?pen:Pens.Gray),0,i*d+d/2,w,i*d+d/2);
			for (i=0;i<MAXNOTE;i++)
				e.Graphics.DrawLine((i%4==3?pen:Pens.Gray),i*DX+DX/2,0,i*DX+DX/2,h);
		}
		using (GraphicsPath p=new GraphicsPath()) {
			p.AddEllipse(0,0,20,20);
			using (Bitmap bmp=new Bitmap(20,20)) {
				using (Graphics g=Graphics.FromImage(bmp)) {
					g.SetClip(p);
					g.FillEllipse(Brushes.Black,0,0,20,20);
					g.FillEllipse(Brushes.White,4,4,20,20);
						g.DrawEllipse(Pens.Black,0,0,19,19);
				}
				for (i=0;i<MAXNOTE;i++) {
					for (int j=0;j<20;j++)
						if (composizione[i]!=null && composizione[i][j])
							e.Graphics.DrawImage(bmp,new Rectangle(DX*i,d*(19-j),20,20),0,0,20,20,GraphicsUnit.Pixel);
							//e.Graphics.FillEllipse(Brushes.Black,DX*i,d*(19-j),20,20);
				}
			}
		}
	}
	
	void pbKeyboard_OnPaint(object sender,PaintEventArgs e) {
		int w=pbKeyboard.ClientSize.Width,
			h=pbCanvas.ClientSize.Height;
		int d=h/20;
		
		e.Graphics.DrawLine(Pens.Black,w-1,0,w-1,h);
		using (Font font=new Font("Verdana",12, FontStyle.Regular, GraphicsUnit.Pixel)) {
			for (int i=0;i<20;i++) {
				int n=20-i;
				e.Graphics.DrawLine(Pens.Black,0,i*d,w,i*d);
				if (n%7!=3 && n%7!=0)
					e.Graphics.FillRectangle(Brushes.Black,0,i*d-d/4,w/2,d/2);
				e.Graphics.DrawString(((char)(65+((n+1)%7))).ToString(),font,Brushes.Black,w*5/6,i*d+d/2-6);
			}
		}
		e.Graphics.DrawLine(Pens.Black,0,20*d,w,20*d);
	}
	
	void OnPlay(object sender,EventArgs e) {
		if (!playing) {
			pnl.HorizontalScroll.Value=0;
			lastNote=-1;
			for (int i=currentNote;i<MAXNOTE;i++)
				if (composizione[i]!=null)
					foreach (bool b in composizione[i])
						if (b) {
							lastNote=i;
							break;
						}
			if (lastNote==-1) return;
			t.Start();
			if (lastNote>=0) {
				pnl.Enabled=false;
				tsmiPlay.Text="Stop";
				playing=true;
				if (outputDevice.IsOpen)
					outputDevice.Close();
				outputDevice.Open();
				outputDevice.SendProgramChange(Channel.Channel1,Instrument.Glockenspiel);
			}
		}
		else {
			currentNote=0;
			pnl.Enabled=true;
			tsmiPlay.Text="Play";
			playing=false;
			t.Stop();
			pbCanvas.Refresh();
		}
	}
	
	static Pitch NoteFromOrder(int n) {
		int octave=n/7,
			tone=n%7;
		if (tone<3) return Pitch.C3+(octave*12+tone*2);
		return Pitch.C3+(octave*12+tone*2-1);
	}
	
	void t_Tick(object sender,EventArgs e) {
		if (currentNote>lastNote) {
			currentNote=0;
			pnl.Enabled=true;
			tsmiPlay.Text="Play";
			playing=false;
			t.Stop();
			pbCanvas.Refresh();
		}
		else {
			if (currentNote==0)
				pnl.HorizontalScroll.Value=0;
			else if (currentNote*DX>pnl.HorizontalScroll.Value+pnl.ClientSize.Width)
				pnl.HorizontalScroll.Value=Math.Max(0,currentNote-2)*DX;
			using (Graphics g=pbCanvas.CreateGraphics()) {
				using (Pen pen=new Pen(Color.Red,10))
					g.DrawLine(pen,0,0,currentNote*DX+DX/2,0);
					if (composizione[currentNote]!=null)
						for (int i=0;i<20;i++)
							if (composizione[currentNote][i]) {
								outputDevice.SendNoteOn(Channel.Channel1,NoteFromOrder(i),40);
								g.FillEllipse(Brushes.Green,DX*currentNote,pbCanvas.ClientSize.Height/20*(19-i),20,20);
							}
				}
			++currentNote;
		}
	}
	
	void SetDirty(bool bDirty) {
		this.bDirty=bDirty;
		string sTitle="(Untitled)";
		if (sFileName!=null) 
			sTitle=Path.GetFileNameWithoutExtension(sFileName);
		if (bDirty) sTitle+="*";
		sTitle+=" - Music Box";
		Text=sTitle;
	}
	
	void OnFileNew(object sender,EventArgs e) {		
		if (!CheckAndSave()) return;
		sFileName=null;
		composizione=new BitArray[MAXNOTE];
		SetDirty(false);
	}
	
	void Open(string sFileName) {
		int i;
		this.sFileName=sFileName;
		SetDirty(false);

		composizione=new BitArray[MAXNOTE];
		using (FileStream fs=File.Open(sFileName,FileMode.Open)) {
			try {
				byte[] buf=new byte[4];
				for (i=0;i<MAXNOTE;i++) {
					fs.Read(buf,0,4);
					composizione[i]=new BitArray(buf);
				}
			}
			catch (EndOfStreamException) {}
		}
		pbCanvas.Refresh();
	}

	bool CheckAndSave() {
		if (bDirty) {
			DialogResult dr=MessageBox.Show("Do you want to save?","File modified",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);
			if (dr==DialogResult.Cancel) return false;
			if (dr==DialogResult.Yes) Save();
		}
		return true;
	}
	
	bool Save() {
		if (sFileName==null)
			using (SaveFileDialog sfd=new SaveFileDialog()) {
				sfd.Filter="BXC file (*.bxc)|*.bxc|Any file (*.*)|*.*";
				sfd.FilterIndex=0;
				DialogResult dr=sfd.ShowDialog();
				if (dr!=DialogResult.OK) return false;
				sFileName=sfd.FileName;
			}
		using (FileStream fs=File.Create(sFileName))
			using (BinaryWriter bw=new BinaryWriter(fs))
				foreach (BitArray ba in composizione) {
					UInt32 n=0;
					if (ba!=null)
						for (int i=0;i<20;i++,n>>=1)
							if (ba[i]) n|=1<<21;
					bw.Write(n>>1);
				}
		SetDirty(false);
		return true;
	}
	void OnFileOpen(object sender,EventArgs e) {
		if (!CheckAndSave()) return;
		using (OpenFileDialog ofd=new OpenFileDialog()) {
			ofd.Filter="BXC file (*.bxc)|*.bxc|Any file (*.*)|*.*";
			if (ofd.ShowDialog()!=DialogResult.OK) return;
			Open(ofd.FileName);
		}
	}
	
	void OnFileSaveAs(object sender,EventArgs e) {
		string sFileName=this.sFileName;
		this.sFileName=null;
		if (!Save()) this.sFileName=sFileName;
	}
	
	void OnFileSave(object sender,EventArgs e) {
		Save();
	}

	void OnFilePrint(object sender,EventArgs e) {
		PrintDoc pd=new PrintDoc(composizione,Path.GetFileNameWithoutExtension(sFileName));
		PrintDialog dlg=new PrintDialog();
		dlg.Document=pd;
		if (dlg.ShowDialog()==DialogResult.OK) 
			pd.Print();
	}
	
	void AddMenuItem(ToolStripMenuItem tsmi,string sText,string sBitmap,EventHandler eh,Keys keys) {
		Bitmap bmp=null;
		if (sBitmap!=null) {
			bmp=new Bitmap(GetType().Assembly.GetManifestResourceStream(sBitmap));
			bmp.MakeTransparent(Color.Magenta);
		}
		tsmi.DropDownItems.Add(new ToolStripMenuItem(sText,bmp,eh,keys));
	}
	
	void OnClosing(object sender,CancelEventArgs e) {
		e.Cancel=!CheckAndSave();
	}
	
	App() {
		Icon=new Icon(GetType(),"musicbox.ico");
		t.Interval=200;
		t.Tick+=t_Tick;
		
		SetDirty(false);
		Size=new Size(800,600);		
		MainMenuStrip=new MenuStrip();
		MainMenuStrip.Items.Add(tsmiFile);
		tsmiPlay=new ToolStripMenuItem("&Play",null,OnPlay,Keys.F5);
		MainMenuStrip.Items.Add(tsmiPlay);
		
		ToolStripLabel tsl=new ToolStripLabel();
		TrackBar tb=new TrackBar();
		tb.ValueChanged+=(s,e) => { tsl.Text=tb.Value.ToString(); t.Interval=1000*60/tb.Value; };
		tb.Minimum=120;
		tb.Maximum=480;
		tb.SmallChange=10;
		tb.LargeChange=60;
		tb.TickStyle=TickStyle.None;
		tb.Value=240;
		ToolStripControlHost tsch=new ToolStripControlHost(tb);

		MainMenuStrip.Items.Add(new ToolStripSeparator());
		MainMenuStrip.Items.Add(new ToolStripLabel("&Tempo"));
		MainMenuStrip.Items.Add(tsch);
		MainMenuStrip.Items.Add(tsl);
		
		AddMenuItem(tsmiFile,"&New","new.bmp",OnFileOpen,Keys.Control|Keys.N);
		AddMenuItem(tsmiFile,"&Open...","open.bmp",OnFileOpen,Keys.Control|Keys.O);
		AddMenuItem(tsmiFile,"&Save","save.bmp",OnFileSave,Keys.Control|Keys.S);
		AddMenuItem(tsmiFile,"Save &as...",null,OnFileSaveAs,Keys.None);
		AddMenuItem(tsmiFile,"Print",null,OnFilePrint,Keys.Control|Keys.P);
		Closing+=OnClosing;
		Resize+=delegate { pbKeyboard.Refresh(); };
		Load+=delegate { if (sFileName!=null) Open(sFileName); };
		
		pnl.BackColor=Color.LightGray;
		pnl.Dock=DockStyle.Fill;
		pnl.Resize+=delegate { 
			pbCanvas.Size=new Size(DX*MAXNOTE,pnl.ClientSize.Height); 
			pbCanvas.Invalidate();
		};
		pnl.Controls.Add(pbCanvas);
		pnl.AutoScroll=true;
		
		pbKeyboard.BackColor=Color.White;
		pbKeyboard.Size=new Size(100,1);
		pbKeyboard.Dock=DockStyle.Left;
		pbKeyboard.Paint+=pbKeyboard_OnPaint;
		pbKeyboard.Resize+=delegate { pbKeyboard.Refresh(); };
		
		pbCanvas.BackColor=Color.Beige;
		pbCanvas.MouseMove+=pbCanvas_MouseMove;
		pbCanvas.MouseUp+=pbCanvas_MouseUp;
		pbCanvas.MouseLeave+=pbCanvas_MouseLeave;
		pbCanvas.Paint+=pbCanvas_OnPaint;
		Controls.AddRange(new System.Windows.Forms.Control[]{pnl,pbKeyboard, MainMenuStrip});
	}
	
	[STAThread]
	static void Main(string[] args) {
		Application.EnableVisualStyles();
		App app=new App();
		if (args.Length==1)
			app.sFileName=args[0];
		Application.Run(app);
	}
}