import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'scheduler-timerange',
    templateUrl: './timerange.component.html',
    styleUrls: ['./timerange.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TimerangeComponent implements OnInit {
    @Input() timeRange: FormGroup;
    @Input() label: string;

    ngOnInit() {

    }
}
