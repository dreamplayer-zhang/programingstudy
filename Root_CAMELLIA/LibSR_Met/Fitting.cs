using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Root_CAMELLIA.LibSR_Met
{
    public delegate Vector<double> ResidualFunc(int iter, double[][] ppIterParam, List<List<double>> ppInput, bool bEstimateRForCircle = true, double radius = 0);
    public delegate Matrix<double> JacobianFunc(int iter, double[][] ppIterParam, List<List<double>> ppInput, bool bEstimateRForCircle = true);

    
    public class Fitting
    {
        // Delegate Func - Residual
        public Vector<double> SetResidual(int iter, double[][] ppIterParam, List<List<double>> ppInput, bool bEstimateRForCircle, double radius, ResidualFunc resFunc)
        {
            return resFunc(iter, ppIterParam, ppInput, bEstimateRForCircle, radius);
        }

        // Residual Func - 1
        public Vector<double> CircleResidual(int iter, double[][] ppIterParam, List<List<double>> ppInput, bool bEstimateRForCircle = true, double radius = 0)
        {
            int nData = ppInput.Count;
            var RR = Vector<double>.Build;
            var R = RR.Dense(nData);

            for (int i = 0; i < nData; i++)
			{
                if (bEstimateRForCircle)
                    R[i] = Math.Sqrt(Math.Pow(ppInput[i][0] - ppIterParam[iter][0], 2) + Math.Pow(ppInput[i][1] - ppIterParam[iter][1], 2)) - ppIterParam[iter][2];
                else
                    R[i] = Math.Sqrt(Math.Pow(ppInput[i][0] - ppIterParam[iter][0], 2) + Math.Pow(ppInput[i][1] - ppIterParam[iter][1], 2)) - radius;
			}

            return R;
        }

        // Residual Func -2
        public Vector<double> GaussianRedsidual(int iter, double[][] ppIterParam, List<List<double>> ppInput, bool bEstimateRForCircle = false, double radius = 0)
        {
            int nData = ppInput.Count;
            var RR = Vector<double>.Build;
            var R = RR.Dense(nData);

            for (int i = 0; i < nData; i++)
            {
                R[i] = Gaussian(ppIterParam[iter][0], ppIterParam[iter][1], ppIterParam[iter][2], ppInput[i][0]) - ppInput[i][1];
            }

            return R;
        }

        public double Gaussian(double a, double b, double c, double x)
        {
            return (c / (Math.Sqrt(2 * Math.PI) * b)) * Math.Exp(-Math.Pow((x - a), 2) / (2 * Math.Pow(b, 2)));
        }



        // Delegate Func - Jacobian
        public Matrix<double> SetJacobian(int iter, double[][] ppIterParam, List<List<double>> ppInput, bool bEstimateRForCircle, JacobianFunc jacFunc)
        {
            return jacFunc(iter, ppIterParam, ppInput, bEstimateRForCircle);
        }

        // Jacobian Func - 1
        public Matrix<double> CircleJacobian(int iter, double[][] ppIterParam, List<List<double>> ppInput, bool bEstimateRForCircle = true)
        {
            int nData = ppInput.Count;
            int nParam = ppIterParam[0].Length;
            var tmp = Matrix<double>.Build;
            var J = tmp.Dense(nData, nParam);

            for (int i = 0; i < nData; i++)
            {
                J[i, 0] = (ppIterParam[iter][0] - ppInput[i][0]) / Math.Sqrt(Math.Pow(ppInput[i][0] - ppIterParam[iter][0], 2) + Math.Pow(ppInput[i][1] - ppIterParam[iter][1], 2));
                J[i, 1] = (ppIterParam[iter][1] - ppInput[i][1]) / Math.Sqrt(Math.Pow(ppInput[i][0] - ppIterParam[iter][0], 2) + Math.Pow(ppInput[i][1] - ppIterParam[iter][1], 2));
                if (bEstimateRForCircle)
                    J[i, 2] = -1; 
            }
        
            return J;
        }

        // Jacobian Func - 2
        public Matrix<double> GaussianJacobian(int iter, double[][] ppIterParam, List<List<double>> ppInput, bool bEstimateRForCircle = false)
        {
            int nData = ppInput.Count;
            int nParam = ppIterParam[0].Length;
            var tmp = Matrix<double>.Build;
            var J = tmp.Dense(nData, nParam);
            double delta = 0.000000001; // 미분할때 극소 변화량

            for (int i = 0; i < nData; i++)
            {
                J[i, 0] = (Gaussian(ppIterParam[iter][0] + delta, ppIterParam[iter][1], ppIterParam[iter][2], ppInput[i][0]) - Gaussian(ppIterParam[iter][0], ppIterParam[iter][1], ppIterParam[iter][2], ppInput[i][0])) / delta;
                J[i, 1] = (Gaussian(ppIterParam[iter][0], ppIterParam[iter][1] + delta, ppIterParam[iter][2], ppInput[i][0]) - Gaussian(ppIterParam[iter][0], ppIterParam[iter][1], ppIterParam[iter][2], ppInput[i][0])) / delta;
                J[i, 2] = (Gaussian(ppIterParam[iter][0], ppIterParam[iter][1], ppIterParam[iter][2] + delta, ppInput[i][0]) - Gaussian(ppIterParam[iter][0], ppIterParam[iter][1], ppIterParam[iter][2], ppInput[i][0])) / delta;
            }

            return J;
        }

        // 호출 함수 : (반복횟수, 파라미터 개수, 초기 좌표, 관찰한 데이터 좌표, 옵티마이저 방법, 피팅 모델, 원에서 R 피팅 할 지 말 지, 피팅 안할거면 반지름 크기)
        public List<double> LineFitting(int nIterNum, int nParam, List<double> pInit, List<List<double>> ppInput,                               // # of nParam == # of pInit
                                                           string strOptMethod, string strFittingModel, bool bEstimateRForCircle, double radius = 0)
        {

            if (nParam != pInit.Count)
            {
                //MessageBox.Show("할당한 파라메터 개수와 초기값에서의 파라메터 개수가 맞지 않습니다.");
                MessageBox.Show("Assigned parameter number and initial parameter number do not match");
                return null;
            }

            ResidualFunc ResidualFit = new ResidualFunc(CircleResidual);
            JacobianFunc JacobianFit = new JacobianFunc(CircleJacobian);

            switch (strFittingModel)        // 추가 후 위에 함수도 수정 가능
            {
                case "Circle":
                    ResidualFit = new ResidualFunc(CircleResidual);
                    JacobianFit = new JacobianFunc(CircleJacobian);
                    break;
                case "Gaussian" :
                    ResidualFit = new ResidualFunc(GaussianRedsidual);
                    JacobianFit = new JacobianFunc(GaussianJacobian);
                    break;
            }

            // 2d Array : nIterNum , nParam
            double[][] ppIterParam = new double[nIterNum + 1][];
            for (int n = 0; n < nIterNum + 1; n++)
            {
                ppIterParam[n] = new double[nParam];
            }

            // 파라메터 초기값 설정
            for(int n = 0; n < nParam; n++)
            {
                ppIterParam[0][n] = pInit[n];
            }

            double mu = 1;                     // mu는 반복할 때, Gap 을 조정함 Gap에 따라 a,b,c 값의 발산을 조정할 수 있음, 초기 mu값은 크게 가져가다가 Fitting 값이 좋아지면 서서히 줄여가는 방법은 추후 적용
            int nData = ppInput.Count;  // data 갯수

            for (int k = 0; k < nIterNum; k++)
            {
                // vector R 만들기
                Vector<double> R = SetResidual(k, ppIterParam, ppInput, bEstimateRForCircle, radius, ResidualFit);

                // Jacobian Matrix J 만들기
                Matrix<double> J = SetJacobian(k, ppIterParam, ppInput, bEstimateRForCircle, JacobianFit);

                // vector P
                var PP = Vector<double>.Build;
                var P = PP.Dense(nParam);

                var DD = Matrix<double>.Build;
                var D = DD.Dense(nParam, nParam, 0.0); // Diagonal Matrix, N x N matrix 생성, 0.0으로 채워짐
                for (int i = 0; i < nParam; i++)
                {
                    for (int j = 0; j < nParam; j++)
                    {
                        if (i != j) D[i, j] = 0;
                        else if (i == j) D[i, j] = J.TransposeThisAndMultiply(J)[i, j];
                    }
                }

                switch (strOptMethod)
                {
                    case "GradientDescent":         // step size 설정해줘야 하는 까다로움이 있음 - backtracking line search 고려 필요
                        double stepSize = 0.01;
                        P = stepSize * 2 * J.TransposeThisAndMultiply(R);
                        break;
                    case "GaussNewton" :
                        P = J.TransposeThisAndMultiply(J).Inverse() * J.TransposeThisAndMultiply(R); 
                        //P = J.TransposeThisAndMultiply(J).Cholesky().Solve(J.TransposeThisAndMultiply(R)); 
                        break;
                    case "LMA" :
                        P = (J.TransposeThisAndMultiply(J) + mu * D).Inverse() * J.TransposeThisAndMultiply(R);
                        break;

                }

                for (int n = 0; n < nParam; n++ )
                    ppIterParam[k + 1][n] = ppIterParam[k][n] - P[n];

            }


            // a0, b0, c0를 계산된 값으로 update
            List<double> resultParam = new List<double>();

            for (int n = 0; n < nParam; n++ )
                resultParam.Add(ppIterParam[nIterNum][n]);

            return resultParam;

        } 

    }
}
