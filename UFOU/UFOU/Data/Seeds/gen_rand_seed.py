import json 
from random import randrange, choice
from datetime import datetime, timedelta

# There are a lot of states
states = """
        Alabama,Alaska,Arizona,Arkansas,California,
        Colorado,Connecticut,Delaware,Florida,Georgia,
        Hawaii,Idaho,Illinois,Indiana,Iowa,Kansas,
        Kentucky,Louisiana,Maine,Maryland,Massachusetts,
        Michigan,Minnesota,Mississippi,Missouri,Montana,
        Nebraska,Nevada,New Hampshire,New Jersey,New Mexico,
        New York,North Carolina,North Dakota,Ohio,Oklahoma,
        Oregon,Pennsylvania,Rhode Island,South Carolina,South Dakota,
        Tennessee,Texas,Utah,Vermont,Virginia,Washington,West Virginia,
        Wisconsin,Wyoming
        """

def main():
    """
    Short program that generates a random list of Report objects, serializes them as Json, and stores them in a seed file
    """
    with open("report_seed.json","w") as report_file:
        report_file.write("[\n")
        for _ in range(20):

            # generate a bunch of random fields
            state = choice(states.split(",")).strip()
            shape = choice(range(21))
            dur = "%i %s" % (choice(range(1,30)), choice("seconds,minutes,hours,days".split(",")))
            desc = "UFO seen in %s that lasted %s" % (state, dur)
            date_occ = datetime(year=randrange(2000, 2020), month=randrange(1, 13), day=randrange(1, 31)) # random date in the last 2 decades
            date_sub = date_occ + timedelta(days=randrange(365)) # account of event likely submitted within a year of it occuring
            approved = choice([True]*5 + [False])
            if approved:
                date_post = date_sub + timedelta(days=randrange(30)) # we the admins likely got around to approving it within a month, if at all
            else:
                date_post = None

            r = {"Location": state,
                "Shape": shape,
                "Duration": dur,
                "Description": desc,
                "DateOccurred": date_occ.strftime("%Y/%m/%d"),
                "DateSubmitted": date_sub.strftime("%Y/%m/%d"),
                "DatePosted": date_post.strftime("%Y/%m/%d") if date_post is not None else "",
                "Approved": approved}
            
            # serialize and write to json file
            report_file.write("\t"+json.dumps(r, indent=2) + ",\n")
        report_file.write("]")
            


if __name__ == "__main__":
    main()