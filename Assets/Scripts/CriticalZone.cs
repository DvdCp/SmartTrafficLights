using UnityEngine;

public class CriticalZone : MonoBehaviour
{
    private const string NorthGenerated = "north";
    private const string SouthGenerated = "south";

    private AgentManager _manager;

    [SerializeField] private Data data;
    [SerializeField] private UIController uiController;

    public bool Accident { get; set; }
    public int TotalCarPassed { get; set; }

    void Awake()
    {
        Reset();

        _manager = GetComponentInParent<AgentManager>();
    }

    public void Reset()
    {
        TotalCarPassed = 0;
        Accident = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        CarMover mover = other.GetComponent<CarMover>();

        if (mover != null)
        {
            mover.RedZone = this;
            mover.Semaphore.CarsFromThisSemaphore++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CarMover mover = other.GetComponent<CarMover>();

        if (mover != null)
        {
            TotalCarPassed++;
            mover.Semaphore.CarsFromThisSemaphore--;

            _manager.OnGoal();
            data.TotalNGoal++;

            if (mover.tag.Equals(NorthGenerated))
                data.NGoalNorth++;

            if (mover.tag.Equals(SouthGenerated))
                data.NGoalSouth++;

            data.TotalAvgTimeToPass += mover.TimeToPass;

            if (mover.tag.Equals(NorthGenerated))
                data.AvgTimeToPassNorth += mover.TimeToPass;

            if (mover.tag.Equals(SouthGenerated))
                data.AvgTimeToPassSouth += mover.TimeToPass;

            data.TotalAvgTimeWaited += mover.TimeWaited;

            if (mover.tag.Equals(NorthGenerated))
                data.AvgTimeWaitedNorth += mover.TimeWaited;

            if (mover.tag.Equals(SouthGenerated))
                data.AvgTimeWaitedSouth += mover.TimeWaited;
        }
    }

    public void DOavgData()
    {
        data.TotalAvgTimeToPass /= TotalCarPassed;
        uiController.totalAvgTimeToPassValue.text = data.TotalAvgTimeToPass.ToString("0.##");

        data.AvgTimeToPassNorth /= data.NGoalNorth;
        uiController.avgTimeToPassNorthValue.text = data.AvgTimeToPassNorth.ToString("0.##");

        data.AvgTimeToPassSouth /= data.NGoalSouth;
        uiController.avgTimeToPassSouthValue.text = data.AvgTimeToPassSouth.ToString("0.##");

        data.TotalAvgTimeWaited /= TotalCarPassed;
        uiController.totalAvgTimeWaitedValue.text = data.TotalAvgTimeWaited.ToString("0.##");

        data.AvgTimeWaitedNorth /= data.NGoalNorth;
        uiController.avgTimeWaitedNorthValue.text = data.AvgTimeWaitedNorth.ToString("0.##");

        data.AvgTimeWaitedSouth /= data.NGoalSouth;
        uiController.avgTimeWaitedSouthValue.text = data.AvgTimeWaitedSouth.ToString("0.##");
    }
}