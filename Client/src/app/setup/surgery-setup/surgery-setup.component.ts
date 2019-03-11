import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';

@Component({
  selector: 'scheduler-surgery-setup',
  templateUrl: './surgery-setup.component.html',
  styleUrls: ['./surgery-setup.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SurgerySetupComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
