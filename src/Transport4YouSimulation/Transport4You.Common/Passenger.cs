using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Transport4YouSimulation
{
    public class Passenger : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ulong CellPhoneAddress
        {
            get;
            set;
        }

        private RoadStop _startStop;
        public RoadStop StartStop
        {
            get { return _startStop; }
            set
            {
                _startStop = value;
                OnPropertyChanged("StartStop");
            }
        }

        private RoadStop _destinationStop;
        public RoadStop DestinationStop
        {
            get { return _destinationStop; }
            set
            {
                _destinationStop = value;
                OnPropertyChanged("DestinationStop");
            }
        }        

        private string _message;
        public string Message
        {
            get{return _message;}
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }    

        private List<string> ActionList
        {
            get;
            set;
        }

        public Passenger(ulong id, RoadStop start, RoadStop dest, List<string> actions)
        {
            this.CellPhoneAddress = id;
            this.StartStop = start;
            this.DestinationStop = dest;
            this.ActionList = new List<string>();
            this.Message = "Message History:\n";

            foreach (string action in actions)
            {
                if (action[0].Equals('['))
                {
                    continue;
                }
                else if (action.Equals("cross"))
                {
                    ActionList.Add(action);
                }
                else
                {
                    ActionList.Add("Line" + action.Substring(8));
                }
            }

            if (NextAction().Equals("cross"))
            {
                start.Opposite.Passengers.Add(this);
                TakeBusOneStop();
            }
            else
            {
                start.Passengers.Add(this);
            }
        }

        public string NextAction()
        {
            if (ActionList.Count > 0)
                return ActionList.ElementAt<string>(0);
            else return "NA";
        }

        public void TakeBusOneStop()
        {
            if (ActionList.Count > 0)
            {
                ActionList.RemoveAt(0);
            }
            if (NextAction().Equals("cross"))
            {
                //Not Implemented!
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return ""+CellPhoneAddress;
        }
    }
}
