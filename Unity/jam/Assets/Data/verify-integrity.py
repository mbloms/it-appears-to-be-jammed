import csv

# intersections
intersections = None
with open('intersections.csv') as csvDataFile:
    csvReader = csv.reader(csvDataFile, delimiter=',')
    intersections = [(row[0], row[1]) for row in csvReader]

# roads
roads = None
with open('roads.csv') as csvDataFile:
    csvReader = csv.reader(csvDataFile, delimiter=',')
    roads = [((row[0], row[1]), (row[2], row[3])) for row in csvReader]

# verify that both end points of all roads are valid intersections
for inter1, inter2 in roads:
    if not inter1 in intersections:
        print('unknown intersection: %s' % str(inter1))
    if not inter2 in intersections:
        print('unknown intersection: %s' % str(inter2))
