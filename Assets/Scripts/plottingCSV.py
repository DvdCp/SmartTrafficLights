import sys
import pandas as pd
import matplotlib.pyplot as plt


def operateFile(filePath=None):
    if filePath == None:
        sys.exit(-1)

    fpWithoutExtension = filePath[:filePath.find('.csv')]

    data = pd.read_csv(filePath)

    NEpisode = data["NEpisode"]
    EpisodeLenght = data["EpisodeLenght"]
    EpisodeSteps = data["EpisodeSteps"]

    Accident = data["Accident"]

    TotalNGoal = data["TotalNGoal"]
    NGoalNorth = data["NGoalNorth"]
    NGoalSouth = data["NGoalSouth"]

    TotalNCarGenerated = data["TotalNCarGenerated"]
    NCarGeneratedNorth = data["NCarGeneratedNorth"]
    NCarGeneratedSouth = data["NCarGeneratedSouth"]

    TotalNCarBeenWaiting = data["TotalNCarBeenWaiting"]
    NCarBeenWaitingNorth = data["NCarBeenWaitingNorth"]
    NCarBeenWaitingSouth = data["NCarBeenWaitingSouth"]

    TotalAvgTimeToPass = data["TotalAVGTimeToPass"]
    AvgTimeToPassNorth = data["AVGTimeToPassNorth"]
    AvgTimeToPassSouth = data["AVGTimeToPassSouth"]

    TotalAvgTimeWaited = data["TotalAVGTimeWaited"]
    AvgTimeWaitedNorth = data["AVGTimeWaitedNorth"]
    AvgTimeWaitedSouth = data["AVGTimeWaitedSouth"]

    x = list(NEpisode)

    y = list(EpisodeLenght)
    plt.plot(x, y, color='#466687', linestyle='-.', marker='*', label="Episode lenght")
    plt.legend()

    y = list(TotalAvgTimeWaited)
    plt.plot(x, y, color='#d1642d', linestyle='--', marker='o', label="Time waited")
    plt.legend()

    y = list(TotalAvgTimeToPass)
    plt.plot(x, y, color='#e2d4b7', linestyle='-', marker='^', label="AVG time to pass")
    plt.legend()

    plt.title("Episode times")
    plt.xlabel("Number of episode")
    plt.ylabel("Time (s)")

    plt.savefig(fpWithoutExtension + "_Times.png")
    plt.close()

    y = list(TotalNCarGenerated)
    plt.plot(x, y, color='#166dc2', linestyle='-.', marker='*', label="Car generated")
    plt.legend()

    y = list(TotalNGoal)
    plt.plot(x, y, color='#00b1b8', linestyle='--', marker='o', label="Car passed to the other side")
    plt.legend()

    y = list(TotalNCarBeenWaiting)
    plt.plot(x, y, color='#a056b2', linestyle='-', marker='^', label="Car that has been waiting some time")
    plt.legend()

    plt.title("Car numbers")
    plt.xlabel("Number of episode")
    plt.ylabel("Cars")

    plt.savefig(fpWithoutExtension + "_Cars.png")
    plt.close()

    y = list(TotalAvgTimeToPass - TotalAvgTimeWaited)
    plt.plot(x, y, color='#466687', linestyle='-.', marker='*', label="Precision")
    plt.legend()

    plt.title("Episode precisions")
    plt.xlabel("Number of episode")
    plt.ylabel("Precision")

    plt.savefig(fpWithoutExtension + "_Precisions.png")
    plt.close()


operateFile(sys.argv[1])
