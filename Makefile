CSFLAGS = -d:debug
CC = dmcs

all : TestRTree.exe

SpatialData.dll : IData.cs SpatialData.cs
	$(CC) $(CSFLAGS) -t:library $^ -out:$@

TestRTree.exe : SpatialData.dll TestRTree.cs RTree.cs
	$(CC) $(CSFLAGS) -r:Mono.C5,SpatialData.dll TestRTree.cs RTree.cs -out:$@

clean:
	rm *.dll
	rm *.exe
