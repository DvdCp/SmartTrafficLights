using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Collections;
using Unity.MLAgents.Actuators;

public class AgentManager : Agent
{
    private const int NorthSemaphore = 0;
    private const int SouthSemaphore = 1;
    
    private bool _otherGenerator;
    private float _episodeTimer;
    private int _totalMaxCarSpawn;

    private CriticalZone _redZone;
    private Reporter _reporter;
    private Data _data;
    private UIController _uiController;
    private SemaphoreController[] _semaphores;

    [SerializeField] private float semaphoreTimer;
    [SerializeField] [Range(0.1f, 5f)] private float timerForDecision;

    /*
     * Funzione di inizializzazione della simulazione, vengono catturati alcuni riferimenti
     */
    public override void Initialize()
    {
        GettingSettingsFromMenu();

        _uiController = GetComponentInChildren<UIController>();
        _uiController.maxTotalNCarGeneratedValue.text = _totalMaxCarSpawn.ToString();

        _data = GetComponentInChildren<Data>();
        _reporter = GetComponentInChildren<Reporter>();

        _redZone = GetComponentInChildren<CriticalZone>();
        _semaphores = GetComponentsInChildren<SemaphoreController>();

        //Inizializzato il numero di episodi da registrato della simulazione
        _data.NEpisode = 0;

        //Creazione del file di report della simulazione
        _reporter.CreateReport();
    }

    /*
     * Funzione lanciata ogni inizio episodio per il reset di alcune dinamiche
     */
    public override void OnEpisodeBegin()
    {
        _uiController.EpisodeReset();

        _uiController.nEpisodeValue.text = _data.NEpisode.ToString();

        //Variabile di controllo per il doppio generatore di auto
        _otherGenerator = false;

        StartCoroutine(nameof(Check4Decision));

        //Reset del timer di episodio
        _episodeTimer = 0;

        //Reset della zona critica nella situazione di partenza
        _redZone.Reset();

        //Reset dei dati registrati nell'episodio precedente
        _data.Reset();

        //Reset dei semafori nella situazione di partenza
        foreach (var semaphore in _semaphores)
            semaphore.Reset();
    }

    /*
     * Funzione lanciata ad ogni frame per il controllo della scena
     */
    void Update()
    {
        //Timer dell'episodio che viene decrementato
        _episodeTimer += Time.deltaTime;

        //Controllo dei timer per ogni semaforo, di modo che ogni volta che delle auto arrivano in coda, scatti questo
        //timer che in caso arrivi a 0 aggiunga una penalità alla IA
        foreach (var semaphore in _semaphores)
        {
            if (!semaphore.IsGreen && semaphore.CarsOnRilevator > 0)
                semaphore.SemaphoreTimer -= (Time.deltaTime * semaphore.CarsOnRilevator);
            else
                semaphore.SemaphoreTimer = semaphoreTimer;

            if (semaphore.SemaphoreTimer < 0f)
                AddReward(-0.1f * semaphore.CarsOnRilevator);
        }

        //Controllo della zona critica e della possibile creazione di incidenti, nel caso l'episodio finisce
        if (_redZone.Accident)
            OnEnd(true);

        //Controllo delle auto generate totali, se queste raggiungono il massimo l'episodio termina una vota che sono passate
        if (_data.TotalNCarGenerated == _totalMaxCarSpawn && _data.TotalNGoal == _totalMaxCarSpawn)
            OnEnd();
    }

    /*
     * Funzione lanciata ad ogni ciclo osservazione - decisione - azione della IA, per collezionare osservazioni
     */
    public override void CollectObservations(VectorSensor sensor)
    {
        //Ogni volta che la IA osserva, questa prende dati sul colore dei semafori, le auto rilevate sui semafori e il 
        //totale delle auto rilevate
        foreach (var semaphore in _semaphores)
        {
            sensor.AddObservation(semaphore.IsGreen);
            sensor.AddObservation(semaphore.CarsOnRilevator);
            sensor.AddObservation(semaphore.CarsFromThisSemaphore);
        }
    }

    /*
     * Funzione chiamata ogni qualvolta alla IA viene dato un input dalla quale si vuole che essa risponda
     */
    public override void OnActionReceived(ActionBuffers actions)
    {
        switch (actions.DiscreteActions[0])
        {
            //Accende semaforo a NORD e spegne semaforo a SUD
            case 0:
                _semaphores[SouthSemaphore].IsGreen = false;
                _semaphores[NorthSemaphore].IsGreen = true;

                _uiController.northSemaphoreValue.color = Color.green;
                _uiController.southSemaphoreValue.color = Color.red;
                break;

            //Accende semaforo a SUD e spegne semaforo a NORD
            case 1:
                _semaphores[NorthSemaphore].IsGreen = false;
                _semaphores[SouthSemaphore].IsGreen = true;

                _uiController.northSemaphoreValue.color = Color.red;
                _uiController.southSemaphoreValue.color = Color.green;
                break;
        }
    }

    /*
     * Funzione di euristica per il funzionamento diretto della IA di scena
     */
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteAction = actionsOut.DiscreteActions;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            discreteAction[0] = 0;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            discreteAction[0] = 1;
    }

    /*
     * Assegnamento reward di goal
     */
    public void OnGoal()
    {
        AddReward(0.1f);
    }

    /*
     * Funzione che controlla se ci sono auto rilevate
     */
    private bool CheckSemaphore()
    {
        foreach (var semaphore in _semaphores)
            if (semaphore.CarsOnRilevator > 0)
                return true;

        return false;
    }

    /*
     * Coroutine che permette alla IA di prendere decisioni ogni tot secondi
     */
    private IEnumerator Check4Decision()
    {
        if (CheckSemaphore())
            RequestDecision();

        yield return new WaitForSeconds(timerForDecision);

        StartCoroutine(nameof(Check4Decision));
    }

    /*
     * Funzione lanciata prima che l'episodio finisca per compiere calcoli finali
     */
    public void OnEnd(bool accident = false, bool generator = false)
    {
        //Controlla nel caso un generatore abbia terminato di generare se anche l'altro è nello stesso stato
        if (generator && !_otherGenerator)
        {
            _otherGenerator = true;
            return;
        }

        StopCoroutine(nameof(Check4Decision));

        //Se non c'è stato alcun incidente al lancio della funzione viene assegnate tale reward
        if (!accident)
            AddReward((float) _redZone.TotalCarPassed /
                      (_semaphores[NorthSemaphore].TotalCarRilevated +
                       _semaphores[SouthSemaphore].TotalCarRilevated));
            
        //Se c'è stato un incidente al lancio della funzione viene assegnata tale penalità e viene registrato
        else
        {
            _data.Accident = true;
            _uiController.nAccidentValue.text = (Int32.Parse(_uiController.nAccidentValue.text) + 1).ToString();
            AddReward(-0.5f);
        }

        //Viene registrato al lancio della funzione quanto tempo è passato dall'inizio dell'episodio
        _data.EpisodeLenght = _episodeTimer;

        //Viene registrato al lancio della funzione quanti step sono stati fatti dall'inizio dell'episodio
        _data.EpisodeSteps = StepCount;

        //Viene lanciata la funzione che fa le medie dei tempi di attravversamento della scena e di aspettativa media
        _redZone.DOavgData();

        //Viene lancia la funzione che scrive effettivamente i dati su file e successivamente li analizza su grafi
        _reporter.WriteData();

        var carMovers = FindObjectsOfType<CarMover>();
        foreach (var car in carMovers)
            Destroy(car.gameObject);
        
        EndEpisode();
    }

    private void GettingSettingsFromMenu()
    {
        if (SkipMenuData.TotalMaxCarSpawn != 0)
            _totalMaxCarSpawn = SkipMenuData.TotalMaxCarSpawn;

        else
            _totalMaxCarSpawn = 10000000;
    }
}