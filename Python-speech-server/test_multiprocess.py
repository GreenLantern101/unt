# for multithreading
import multiprocessing
from multiprocessing import Pool, Process
import time

def cb(result):
    print "thread returned a callback."
    print result

def task(dialog, num):
    # placeholder calculations...
    for i in range(1,100):
        k = 0
        time.sleep(.01)
        for j in range(10**6):
            k+=1
    print "calculations done!"
    return dialog

results = []

def main(pool):
    print "main func:"
    #returns AsyncResults
    #results.append(pool.apply_async(task, ("hello bob",1), callback=cb))
    #results.append(pool.apply_async(task, ("hello joe",2), callback=cb))
    #results.append(pool.apply_async(task, ("hello mary",3), callback=cb))
    a = Process(target=task, args=("hello bob", 1))
    b = Process(target=task, args=("hello joe", 2))
    c = Process(target=task, args=("hello mary", 3))
    a.start()
    b.start()
    c.start()
    results.append(a)
    results.append(b)
    results.append(c)

    print "threads made... processing..."
    #poll results to see if ready
    '''
    for r in results:
        if not r.ready():
            r.get()
    '''
    print "main thread keeps on doing stuff..."


if __name__=="__main__":
    pool = Pool()
    main(pool)
