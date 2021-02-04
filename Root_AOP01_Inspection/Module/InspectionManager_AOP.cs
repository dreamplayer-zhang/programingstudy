using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using RootTools;
using RootTools.Database;
using RootTools_Vision;
using MBrushes = System.Windows.Media.Brushes;
using DPoint = System.Drawing.Point;
using Root_AOP01_Inspection.Recipe;

namespace Root_AOP01_Inspection.Module
{
	public class InspectionManager_AOP : WorkFactory
	{
		/// <summary>
		/// Defect 정보 변경 시 사용할 Event Handler
		/// </summary>
		/// <param name="item">Defect Information</param>
		/// <param name="args">arguments. 필요한 경우 수정해서 사용</param>
		public delegate void ChangeDefectInfoEventHanlder(List<RootTools.Database.Defect> item, Brush brush, Pen pen);
		public delegate void EventHandler();
		///// <summary>
		///// UI에 Defect을 추가하기 위해 발생하는 Event
		///// </summary>
		//public event ChangeDefectInfoEventHanlder AddDefectEvent;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddUIEvent;
		/// <summary>
		/// UI Defect을 지우기 위해 발생하는 Event
		/// </summary>
		public event EventHandler ClearDefectEvent;
		/// <summary>
		/// UI에 추가된 Defect 정보를 새로고침 하기위한 Event
		/// </summary>
		public event EventHandler RefreshDefectEvent;

		public static event EventHandler PellInspectionDone;
		public static event EventHandler PatternInspectionDone;

		#region [Members]
		private readonly AOP_RecipeSurface recipe;
		private SharedBufferInfo sharedBufferinfo;
		#endregion

		#region [Properties]
		public SharedBufferInfo SharedBufferInfo
		{
			get { return sharedBufferinfo; }
		}

		public AOP_RecipeSurface Recipe
		{
			get => recipe;
		}
		#endregion

		public InspectionManager_AOP(AOP_RecipeSurface _recipe, SharedBufferInfo bufferInfo)
		{
			this.recipe = _recipe;
			this.sharedBufferinfo = bufferInfo;
		}


		//public int[] mapdata = new int[14 * 14];




		private new void Start()
		{
			string lotId = "Lotid";
			string partId = "Partid";
			string setupId = "SetupID";
			string cstId = "CSTid";
			string waferId = "WaferID";
			//string sRecipe = "RecipeID";
			string recipeName = recipe.Name;

			DatabaseManager.Instance.SetLotinfo(lotId, partId, setupId, cstId, waferId, recipeName);

			base.Start();
		}

		#region [Overrides]
		protected override void Initialize()
		{
			//CreateWorkManager(WORK_TYPE.ALIGNMENT, 8);
			CreateWorkManager(WORK_TYPE.INSPECTION, 8);
			CreateWorkManager(WORK_TYPE.DEFECTPROCESS, 8);
			CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);
		}

		protected override WorkplaceBundle CreateWorkplaceBundle()
		{
			//RecipeType_WaferMap waferMap = recipe.WaferMap;

			if (recipe.WaferMap == null)
			{
				MessageBox.Show("Map 정보가 없습니다.");
				return null;
			}
			if (recipe.WaferMap.MapSizeX == 0 || recipe.WaferMap.MapSizeY == 0)
			{
				MessageBox.Show("Map 정보가 없습니다.");
				return null;
			}

			WorkplaceBundle workplaceBundle = new WorkplaceBundle();

			return CreateWorkplaceBundle_WaferMap();
		}

		protected override WorkBundle CreateWorkBundle()
		{
			//WorkBundle workBundle = new WorkBundle();

			////Position position = new Position();
			////position.SetParameter(recipe.GetItem<PositionParameter>());

			//BacksideSurface surface = new BacksideSurface();
			//surface.SetParameter(recipe.GetItem<BacksideSurfaceParameter>());

			//ProcessDefect processDefect = new ProcessDefect();
			//ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();


			////workBundle.Add(position);
			//workBundle.Add(surface);
			//workBundle.Add(processDefect);
			//workBundle.Add(processDefect_Wafer);

			////workBundle.SetRecipe(recipe); // Recipe에서?

			//return workBundle;

			List<ParameterBase> paramList = recipe.ParameterItemList;
			WorkBundle bundle = new WorkBundle();

			foreach (ParameterBase param in paramList)
			{
				WorkBase work = (WorkBase)Tools.CreateInstance(param.InspectionType);
				work.SetRecipe(recipe);
				work.SetParameter(param); // 같은 class를 사용하는 parameter 객체가 존재할 수 있으므로 반드시 work를 생성할 때 parameter를 셋팅

				bundle.Add(work);
			}

			ProcessDefect processDefect = new ProcessDefect();
			ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();

			bundle.Add(processDefect);
			bundle.Add(processDefect_Wafer);

			return bundle;
		}

		protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
		{
			works.SetRecipe(recipe);

			return true;
		}

		#endregion

		internal void RefreshDefect()
		{
			//Defect 그리기 새로고침 event발생
			if (RefreshDefectEvent != null)
			{
				RefreshDefectEvent();
			}
		}
		public void SnapDone_Callback(object obj, SnapDoneArgs args)
		{
			//if (this.workplaceBundle == null) return; // 검사 진행중인지 확인하는 조건으로 바꿔야함

			//Rect snapArea = new Rect(new Point(args.startPosition.X, args.startPosition.Y), new Point(args.endPosition.X, args.endPosition.Y));

			//foreach (Workplace wp in this.workplaceBundle)
			//{
			//    Rect checkArea = new Rect(new Point(wp.PositionX, wp.PositionY + wp.BufferSizeY), new Point(wp.PositionX + wp.BufferSizeX, wp.PositionY));

			//    if (snapArea.Contains(checkArea) == true)
			//    {
			//        wp.STATE = WORK_TYPE.SNAP;
			//    }
			//}
		}

		public WorkplaceBundle CreateWorkplaceBundle_WaferMap()
		{
			RecipeType_WaferMap mapInfo = recipe.WaferMap;
			OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
			WorkplaceBundle bundle = new WorkplaceBundle();
			try
			{
				bundle.Add(new Workplace(-1, -1, 0, 0, 0, 0, bundle.Count));

				var wafermap = mapInfo.Data;
				int nSizeX = mapInfo.MapSizeX;
				int nSizeY = mapInfo.MapSizeY;
				int nMasterX = mapInfo.MasterDieX;
				int nMasterY = mapInfo.MasterDieY;
				int nDiePitchX = originRecipe.DiePitchX;
				int nDiePitchY = originRecipe.DiePitchY;

				int nOriginAbsX = originRecipe.OriginX;
				int nOriginAbsY = originRecipe.OriginY - originRecipe.DiePitchY; // 좌상단 기준

				bundle.SizeX = nSizeX;
				bundle.SizeY = nSizeY;

				// Right
				for (int x = nMasterX; x < nSizeX; x++)
				{
					// Top
					for (int y = nMasterY; y >= 0; y--)
					{
						if (wafermap[x + y * nSizeX] == 1)
						{
							int distX = x - nMasterX;
							int distY = y - nMasterY;
							int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
							int nDieAbsY = nOriginAbsY + distY * nDiePitchY;

							Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY, bundle.Count);

							if (y == nMasterY)
							{
								workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
							}

							bundle.Add(workplace);
						}
					}

					// Bottom
					for (int y = nMasterY + 1; y < nSizeY; y++)
					{
						if (wafermap[x + y * nSizeX] == 1)
						{
							int distX = x - nMasterX;
							int distY = y - nMasterY;
							int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
							int nDieAbsY = nOriginAbsY + distY * nDiePitchY;

							Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY, bundle.Count);
							bundle.Add(workplace);
						}
					}
				}


				// Left
				for (int x = nMasterX - 1; x >= 0; x--)
				{
					// Top
					for (int y = nMasterY; y >= 0; y--)
					{
						if (wafermap[x + y * nSizeX] == 1)
						{
							int distX = x - nMasterX;
							int distY = y - nMasterY;
							int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
							int nDieAbsY = nOriginAbsY + distY * nDiePitchY;


							Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY, bundle.Count);

							if (y == nMasterY)
							{
								workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
							}

							bundle.Add(workplace);
						}
					}

					// Bottom
					for (int y = nMasterY + 1; y < nSizeY; y++)
					{
						if (wafermap[x + y * nSizeX] == 1)
						{
							int distX = x - nMasterX;
							int distY = y - nMasterY;
							int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
							int nDieAbsY = nOriginAbsY + distY * nDiePitchY;

							Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY, bundle.Count);
							bundle.Add(workplace);
						}
					}
				}

				bundle.SetSharedBuffer(sharedBufferinfo);
				//this. = bundle;
				return bundle;
			}
			catch (Exception ex)
			{
				throw new ArgumentException("Inspection 생성에 실패 하였습니다.\n", ex.Message);
			}
		}

		internal void AddDefect(List<Defect> defectList)
		{
			if (AddUIEvent != null)
			{
				AddUIEvent(defectList, null, new Pen(Brushes.Red, 2));
			}
		}
		internal void AddRect(List<Defect> list, Brush brush, Pen pen)
		{
			if (AddUIEvent != null)
			{
				AddUIEvent(list, brush, pen);
			}
		}
		internal void ClearDefect()
		{
			if (ClearDefectEvent != null)
			{
				ClearDefectEvent();
			}
		}
		public void InitInspectionInfo()
		{
			string lotId = "Lotid";
			string partId = "Partid";
			string setupId = "SetupID";
			string cstId = "CSTid";
			string waferId = "WaferID";
			//string sRecipe = "RecipeID";
			string recipeName = recipe.Name;
			var temp = DatabaseManager.Instance.GetConnectionStatus();
			DatabaseManager.Instance.SetLotinfo(lotId, partId, setupId, cstId, waferId, recipeName);
		}
		public static unsafe DPoint GetEdge(ImageData img, System.Windows.Rect rcROI, RootTools.Inspects.eEdgeFindDirection eDirection, bool bUseAutoThreshold, bool bUseB2D, int nThreshold)
		{
			// variable
			int nSum = 0;
			double dAverage = 0.0;
			int nEdgeY = 0;
			int nEdgeX = 0;

			// implement

			if (bUseAutoThreshold == true)
			{
				nThreshold = GetThresholdAverage(img, rcROI, eDirection);
			}

			switch (eDirection)
			{
				case RootTools.Inspects.eEdgeFindDirection.TOP:
					for (int i = 0; i < rcROI.Height; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
						else bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
						for (int j = 0; j < rcROI.Width; j++)
						{
							nSum += *bp;
							bp++;
						}
						dAverage = nSum / rcROI.Width;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeY = (int)rcROI.Bottom - i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeY = (int)rcROI.Top + i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case RootTools.Inspects.eEdgeFindDirection.LEFT:
					for (int i = 0; i < rcROI.Width; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
						else bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
						for (int j = 0; j < rcROI.Height; j++)
						{
							nSum += *bp;
							bp += img.p_Stride;
						}
						dAverage = nSum / rcROI.Height;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeX = (int)rcROI.Right - i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeX = (int)rcROI.Left + i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case RootTools.Inspects.eEdgeFindDirection.RIGHT:
					for (int i = 0; i < rcROI.Width; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
						else bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
						for (int j = 0; j < rcROI.Height; j++)
						{
							nSum += *bp;
							bp += img.p_Stride;
						}
						dAverage = nSum / rcROI.Height;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeX = (int)rcROI.Left + i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeX = (int)rcROI.Right - i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case RootTools.Inspects.eEdgeFindDirection.BOTTOM:
					for (int i = 0; i < rcROI.Height; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
						else bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
						for (int j = 0; j < rcROI.Width; j++)
						{
							nSum += *bp;
							bp++;
						}
						dAverage = nSum / rcROI.Width;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeY = (int)rcROI.Top + i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeY = (int)rcROI.Bottom - i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}

						nSum = 0;
					}
					break;
			}

			return new System.Drawing.Point(nEdgeX, nEdgeY);
		}
		/// <summary>
		/// ROI 영역 내의 성분 방향을 획득하여 반환한다
		/// </summary>
		/// <param name="img"></param>
		/// <param name="rcROI"></param>
		/// <returns></returns>
		public static unsafe RootTools.Inspects.eEdgeFindDirection GetDirection(ImageData img, System.Windows.Rect rcROI)
		{
			// variable
			double dRatio = 0.0;
			int nSum = 0;
			double dAverageTemp = 0.0;
			Dictionary<RootTools.Inspects.eBrightSide, double> dic = new Dictionary<RootTools.Inspects.eBrightSide, double>();

			// implement
			// Left
			dRatio = rcROI.Width * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
				for (int j = 0; j < rcROI.Height; j++)
				{
					nSum += *bp;
					bp += img.p_Stride;
				}
			}
			dAverageTemp = nSum / (rcROI.Height * (int)dRatio);
			dic.Add(RootTools.Inspects.eBrightSide.LEFT, dAverageTemp);
			nSum = 0;

			// Top
			dRatio = rcROI.Height * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
				for (int j = 0; j < rcROI.Width; j++)
				{
					nSum += *bp;
					bp++;
				}
			}
			dAverageTemp = nSum / (rcROI.Width * (int)dRatio);
			dic.Add(RootTools.Inspects.eBrightSide.TOP, dAverageTemp);
			nSum = 0;

			// Right
			dRatio = rcROI.Width * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i).ToPointer());
				for (int j = 0; j < rcROI.Height; j++)
				{
					nSum += *bp;
					bp += img.p_Stride;
				}
			}
			dAverageTemp = nSum / (rcROI.Height * (int)dRatio);
			dic.Add(RootTools.Inspects.eBrightSide.RIGHT, dAverageTemp);
			nSum = 0;

			// Bottom
			dRatio = rcROI.Height * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
				for (int j = 0; j < rcROI.Width; j++)
				{
					nSum += *bp;
					bp++;
				}
			}
			dAverageTemp = nSum / (rcROI.Width * (int)dRatio);
			dic.Add(RootTools.Inspects.eBrightSide.BOTTOM, dAverageTemp);
			nSum = 0;

			var maxKey = dic.Keys.Max();
			var maxValue = dic.Values.Max();
			// Value값이 가장 큰 Key값 찾기
			var keyOfMaxValue = dic.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

			if (keyOfMaxValue == RootTools.Inspects.eBrightSide.TOP) return RootTools.Inspects.eEdgeFindDirection.BOTTOM;
			else if (keyOfMaxValue == RootTools.Inspects.eBrightSide.BOTTOM) return RootTools.Inspects.eEdgeFindDirection.TOP;
			else if (keyOfMaxValue == RootTools.Inspects.eBrightSide.LEFT) return RootTools.Inspects.eEdgeFindDirection.RIGHT;
			else return RootTools.Inspects.eEdgeFindDirection.LEFT;
		}


		static unsafe int GetThresholdAverage(ImageData img, System.Windows.Rect rcROI, RootTools.Inspects.eEdgeFindDirection eDirection)
		{
			// variable
			int nSum = 0;
			int nThreshold = 40;

			// implement

			if (eDirection == RootTools.Inspects.eEdgeFindDirection.TOP || eDirection == RootTools.Inspects.eEdgeFindDirection.BOTTOM)
			{
				double dRatio = rcROI.Height * 0.1;
				double dAverage1 = 0.0;
				double dAverage2 = 0.0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
					for (int j = 0; j < rcROI.Width; j++)
					{
						nSum += *bp;
						bp++;
					}
				}
				dAverage1 = nSum / (rcROI.Width * (int)dRatio);
				nSum = 0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
					for (int j = 0; j < rcROI.Width; j++)
					{
						nSum += *bp;
						bp++;
					}
				}
				dAverage2 = nSum / (rcROI.Width * (int)dRatio);
				nSum = 0;
				////////////////////////////////////////////////
				nThreshold = (int)(dAverage1 + dAverage2) / 2;
			}
			else
			{
				double dRatio = rcROI.Width * 0.1;
				double dAverage1 = 0.0;
				double dAverage2 = 0.0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
					for (int j = 0; j < rcROI.Height; j++)
					{
						nSum += *bp;
						bp += img.p_Stride;
					}
				}
				dAverage1 = nSum / (rcROI.Height * (int)dRatio);
				nSum = 0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
					for (int j = 0; j < rcROI.Height; j++)
					{
						nSum += *bp;
						bp += img.p_Stride;
					}
				}
				dAverage2 = nSum / (rcROI.Height * (int)dRatio);
				nSum = 0;
				////////////////////////////////////////////////
				nThreshold = (int)(dAverage1 + dAverage2) / 2;
			}

			return nThreshold;
		}

	}
}
