using Microsoft.Win32;
using Root_AOP01_Inspection.Module;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Root_AOP01_Inspection
{
	public class ProgramManager : MainWindow
	{
		//Single ton
		private ProgramManager()
		{

		}

		private static readonly Lazy<ProgramManager> instance = new Lazy<ProgramManager>(() => new ProgramManager());

		public static ProgramManager Instance
		{
			get
			{
				return instance.Value;
			}
		}

		#region [Setting Parameters]

		private bool IsInitilized = false;
		//Memory 
		//private string memoryPool = "pool";
		//private string memoryGroup = "group";
		//private string memoryNameImage = "mem";
		//private string memoryNameROI = "ROI";

		private string memoryNameROI = "ROI";

		//Recipe
		private string recipeFolderPath = @"C:\Root\Recipe";
		#endregion

		#region [Members]
		private new IDialogService dialogService;

		private AOP01_Engineer engineer = new AOP01_Engineer();
		private MemoryTool memoryTool;
		//private RecipeBase recipe;

		private ImageData imageMain;
		private ImageData image45D;
		private ImageData imageSideLeft;
		private ImageData imageSideTop;
		private ImageData imageSideRight;
		private ImageData imageSideBottom;
		private ImageData roiLayer;

		InspectionManager_AOP inspectionManager;
		#endregion

		#region [Getter Setter]
		public AOP01_Engineer Engineer { get => engineer; private set => engineer = value; }
		public MemoryTool MemoryTool { get => memoryTool; private set => memoryTool = value; }
		//public RecipeBase Recipe { get => recipe; private set => recipe = value; }
		public ImageData ImageMain { get => imageMain; private set => imageMain = value; }
		public ImageData Image45D { get => image45D; private set => image45D = value; }
		public ImageData ImageSideLeft { get => imageSideLeft; private set => imageSideLeft = value; }
		public ImageData ImageSideTop { get => imageSideTop; private set => imageSideTop = value; }
		public ImageData ImageSideRight { get => imageSideRight; private set => imageSideRight = value; }
		public ImageData ImageSideBottom { get => imageSideBottom; private set => imageSideBottom = value; }

		public ImageData ROILayer { get => roiLayer; private set => roiLayer = value; }
		public InspectionManager_AOP InspectionManager { get => inspectionManager; private set => inspectionManager = value; }

		public IDialogService DialogService { get => dialogService; set => dialogService = value; }
		#endregion

		public bool Initialize()
		{
			bool result = true;

			try
			{
				if (IsInitilized == false)
				{
					var eng = InitEngineer("AOP01");
					var memory = InitMemory();
					var mem = InitMember();
					if (eng && memory && mem)
					{
						IsInitilized = true;
						result = true;
					}
					else
					{
						IsInitilized = false;
						result = false;
					}
				}
			}
			catch (Exception)
			{
				IsInitilized = false;
				result = false;
			}

			return result;
		}
		private bool InitEngineer(string name)
		{
			this.engineer.Init(name);
			return true;
		}

		private bool InitMemory()
		{
			memoryTool = this.engineer.ClassMemoryTool();

			imageMain = new ImageData(memoryTool.GetMemory(App.mPool, App.mGroup, App.mMainMem));
			imageMain.p_nByte = memoryTool.GetMemory(App.mPool, App.mGroup, App.mMainMem).p_nCount;
			image45D = new ImageData(memoryTool.GetMemory(App.mPool, App.mGroup, App.m45DMem));
			image45D.p_nByte = memoryTool.GetMemory(App.mPool, App.mGroup, App.m45DMem).p_nCount;
			imageSideLeft = new ImageData(memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideLeftMem));
			imageSideLeft.p_nByte = memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideLeftMem).p_nCount;
			imageSideTop = new ImageData(memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideTopMem));
			imageSideTop.p_nByte = memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideTopMem).p_nCount;
			imageSideRight = new ImageData(memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideRightMem));
			imageSideRight.p_nByte = memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideRightMem).p_nCount;
			imageSideBottom = new ImageData(memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideBotMem));
			imageSideBottom.p_nByte = memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideBotMem).p_nCount;

			roiLayer = new ImageData(memoryTool.GetMemory("pool", "group", memoryNameROI));
			//roiLayer.p_nByte = memoryTool.GetMemory(memoryPool, memoryGroup, memoryNameROI).p_nCount;

			//ImageEdge = engineer.m_handler.m_edgesideVision.GetMemoryData(Module.EdgeSideVision.EDGE_TYPE.EdgeTop);
			//ImageEdge.p_nByte = engineer.m_handler.m_edgesideVision.GetMemoryData(Module.EdgeSideVision.EDGE_TYPE.EdgeTop).p_nByte;


			return true;
		}

		private bool InitMember()
		{
			//recipe=

			if (!Directory.Exists(recipeFolderPath))
				Directory.CreateDirectory(recipeFolderPath);

			// Front
			this.InspectionManager = new InspectionManager_AOP(imageMain.GetPtr(), imageMain.p_Size.X, imageMain.p_Size.Y);

			this.Engineer.InspectionManager = this.InspectionManager;
			//this.Engineer.InspectionManager.Recipe = this.recipe;

			return true;
		}

		#region [Recipe Method]
		public void NewRecipe()
		{
			//try
			//{
			//	if (MessageBox.Show("작성 중인 레시피(Recipe)를 저장하시겠습니까?", "YesOrNo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			//	{
			//		if (this.Recipe.RecipePath == "")
			//		{
			//			ShowDialogSaveRecipe();
			//		}
			//		else
			//		{
			//			this.Recipe.Save(this.Recipe.RecipePath);
			//		}
			//	}

			//	this.Recipe.Clear();
			//	ShowDialogSaveRecipe();

			//	WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
			//}
			//catch (Exception ex)
			//{
			//	MessageBox.Show("Save Recipe : " + ex.Message);
			//}
		}

		public void ShowDialogSaveRecipe()
		{
			try
			{
				SaveFileDialog dlg = new SaveFileDialog();
				dlg.InitialDirectory = recipeFolderPath;

				dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
				if (dlg.ShowDialog() == true)
				{
					string sFilePath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
					string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
					string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
					string sFolderPath = Path.Combine(sFilePath, sFileNameNoExt); // 레시피 이름으로 된 폴더
					string sResultFileName = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

					DirectoryInfo dir = new DirectoryInfo(sFolderPath);
					if (!dir.Exists)
						dir.Create();

					this.SaveRecipe(sResultFileName);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Save Recipe : " + ex.Message);
			}
		}

		public void SaveRecipe(string recipePath)
		{
			string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			using (StreamWriter writer = new StreamWriter(recipePath, true))
			{
				writer.WriteLine(time + " - SaveRecipe()");
			}


			//AOPEventManager.OnBeforeRecipeSave(recipe, new RecipeEventArgs());

			if (recipePath.IndexOf(".rcp") == -1) recipePath += ".rcp";
			//recipe.Save(recipePath);

			//AOPEventManager.OnAfterRecipeSave(recipe, new RecipeEventArgs());
			//this.Load(sFilePath); //?
			//WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
		}

		public void ShowDialogLoadRecipe()
		{
			try
			{
				OpenFileDialog dlg = new OpenFileDialog();
				dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
				if (dlg.ShowDialog() == true)
				{

					// 레시피 전/후처리 이벤트 Recipe 클래스 안쪽에 넣어야할 수 도?
					//AOPEventManager.OnBeforeRecipeRead(recipe, new RecipeEventArgs());

					this.LoadRecipe(dlg.FileName);
					WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());

					//AOPEventManager.OnAfterRecipeRead(recipe, new RecipeEventArgs());

				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Load Recipe : " + ex.Message);
			}
		}
		public void LoadRecipe(string recipePath)
		{
			//string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			//using (StreamWriter writer = new StreamWriter(recipePath, true))
			//{
			//	writer.WriteLine(time + " - LoadRecipe()");
			//}

			//this.recipe.Read(recipePath);
		}

		#endregion
	}
}
