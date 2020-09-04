using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    /// <summary>
    /// Position 
    /// - 전체 이미지 사용
    /// - 결과로 좌표 이동값(trans) 파라매터 전달
    /// PreInspection
    /// - 전체 이미지 사용
    /// - 결과
    /// Inspection
    /// - Inpsection ROI 영역 이미지 사용
    /// 
    /// </summary>
    public enum WORK_TYPE
    {
        None = 0,
        Position,
        PreInspection,
        Inspection,
        Measurement,
        ProcessDefect,
    };

    public interface IWork
    {
        WORK_TYPE TYPE { get; }

    }
}
