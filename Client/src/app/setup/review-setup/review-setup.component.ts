import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { sampleSetup } from '../setup.models';

@Component({
    selector: 'scheduler-review-setup',
    templateUrl: './review-setup.component.html',
    styleUrls: ['./review-setup.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReviewSetupComponent implements OnInit {
    public result: Observable<any>;
    constructor(private httpClient: HttpClient) { }

    ngOnInit() {
    }

    public submitTest(): void {
        const setup = sampleSetup;
        const httpOptions = {
            headers : new HttpHeaders({
                "Content-Type": "application/json"
            })
        };
        this.result = this.httpClient
            .post("http://localhost:5000/api/assignment", setup, httpOptions)
            .pipe(
                map(result => JSON.stringify(result))
            )
    }

}
