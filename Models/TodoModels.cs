using System;
using System.Collections.Generic;

namespace DesktopTodo.Models
{
    // 1. 상태 및 공통 열거형 (Enums)
    public enum TodoState
    {
        NotStarted, // 시작 전
        InProgress, // 진행 중
        Pending,    // 대기 중 (외부 요인 등으로 인해 시작/진행 대기)
        Holding,    // 보류 (일시 정지)
        Completed   // 완료
    }

    public enum RoutineType
    {
        Daily,   // 매일
        Weekly,  // 매주 (특정 요일)
        Monthly  // 매월 (특정 일 또는 말일)
    }

    // 2. 모든 업무의 기본이 되는 추상 클래스 (공통 속성)
    public abstract class TodoItemBase
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // 고유 ID
        public string Title { get; set; } // 업무명
        public string Memo { get; set; } // 상세 메모
        public TodoState State { get; set; } = TodoState.NotStarted; // 현재 상태
        public string ColorTag { get; set; } = "#FFFFFF"; // 컬러 코딩 (Hex 코드 등)
        
        public bool IsRolloverEnabled { get; set; } = false; // 이월(Rollover) 옵션 켜기/끄기
        public DateTime CreatedAt { get; set; } = DateTime.Now; // 생성일
        public DateTime? CompletedDate { get; set; } // 완료된 정확한 날짜 (Q2. 숨김 처리를 위해 필요)
    }

    // 3. 단발성 업무 (Task) 및 루틴에서 파생된 개별 업무
    public class SingleTask : TodoItemBase
    {
        public DateTime TargetDate { get; set; } // 수행해야 하는 특정 날짜
        
        // 만약 이 업무가 루틴(Routine)에 의해 자동 생성된 것이라면, 원본 루틴의 ID를 가짐
        // (독립적인 메모와 상태를 가지면서도, 나중에 루틴을 수정/삭제할 때 추적하기 위함)
        public string ParentRoutineId { get; set; } 
    }

    // 4. 공유형 업무 (Project / 구 Static)
    public class ProjectTask : TodoItemBase
    {
        public DateTime StartDate { get; set; } // 시작일
        public DateTime EndDate { get; set; }   // 마감일 (이 날짜를 기준으로 D-Day 계산)
        
        public int ProgressPercentage { get; set; } = 0; // 진행률 (0 ~ 100%)
        
        // Project 업무 전용 체크리스트 (선택 사항)
        public List<ChecklistItem> SubTasks { get; set; } = new List<ChecklistItem>();
    }

    public class ChecklistItem
    {
        public string Title { get; set; }
        public bool IsDone { get; set; }
    }

    // 5. 반복형 업무 정의 (Routine Definition / 구 Instance)
    // 주의: 이 클래스는 캘린더에 직접 그려지는 게 아니라, SingleTask를 '생성'해주는 공장(Factory) 역할을 합니다.
    public class RoutineDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public string ColorTag { get; set; }
        public bool IsRolloverEnabled { get; set; }

        public RoutineType Type { get; set; }
        
        // Weekly 설정 시 사용 (예: 월, 수, 금)
        public List<DayOfWeek> TargetDaysOfWeek { get; set; } = new List<DayOfWeek>();
        
        // Monthly 설정 시 사용
        public int? MonthlyTargetDate { get; set; } // 예: 15일 (31일 지정 시 예외 처리 필요)
        public bool IsLastDayOfMonth { get; set; } = false; // Q1. 말일(Last Day) 옵션
    }

    // 6. 앱 설정 (Settings)
    public class AppSettings
    {
        // Q2. Project 조기 완료 시 미래 날짜 캘린더에서 숨길지 여부
        // true: 완료된 다음 날부터는 캘린더 빈칸 처리 (공간 확보)
        // false: EndDate까지 계속 완료 줄긋기 상태로 표시
        public bool HideFutureCompletedProjects { get; set; } = true; 
        
        // UI 테마, 투명도 등 (추후 추가)
        public double WindowOpacity { get; set; } = 0.8;
    }
}